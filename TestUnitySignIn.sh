#!/bin/bash

# Inputs
unityEmail=`jq -rn --arg str "$1" '$str | @uri'`
unityPass=`jq -rn --arg str "$2" '$str | @uri'`

newVersion=$3

# Start conversation
conversationStarterResponse=$(curl -s -D - -o /dev/null -L --max-redirs 2 https://publisher.unity.com/)

conversationId=$(echo "$conversationStarterResponse" | grep -i '^set-cookie: conversationId=' | sed -E 's/.*conversationId=([^;]+).*/\1/')
cookie="conversationId=$conversationId"

# Get csrf Token
csrfResponse=$(curl -s -D - -X GET \
	-H "Cookie: $cookie" \
	https://login.unity.com/api/auth/csrf)

csrfToken=$(echo $csrfResponse | grep -o '{.*}'| jq -r '.csrfToken')
csrfCookie=$(echo "$csrfResponse" | grep -i '^set-cookie: __Host-authjs.csrf-token=' | sed -E 's/.*__Host-authjs.csrf-token=([^;]+).*/\1/')
cookie+="; __Host-authjs.csrf-token=$csrfCookie"

# Get session token
credentialsBody="email=$unityEmail&password=$unityPass&csrfToken=$csrfToken&callbackUrl=https%3A%2F%2Flogin.unity.com%2Fen%2Fsign-in"
credentialsResponse=$(curl -s -D - -X POST \
	-H "Content-Type: application/x-www-form-urlencoded" \
	-H "Cookie: $cookie" \
	-d "$credentialsBody" \
	https://login.unity.com/api/auth/callback/credentials)

sessionToken=$(echo "$credentialsResponse" | grep -i '^set-cookie: __Secure-authjs.session-token=' | sed -E 's/.*__Secure-authjs.session-token=([^;]+).*/\1/')
cookie+="__Secure-authjs.session-token=$sessionToken"

# Authorise
authoriseResponse=$(curl -s -D - -X GET \
	-H "Cookie: $cookie" \
	-L --max-redirs 1 \
	https://login.unity.com/en/sign-in)

hashedUserId=$(echo "$authoriseResponse" | grep -i '^set-cookie: _hashed_user_id=' | sed -E 's/.*_hashed_user_id=([^;]+).*/\1/')
cookie+="_hashed_user_id=$hashedUserId"

# Get the final values
finalUrl=$(echo "$authoriseResponse" | grep -oP "(?<=<a href=')[^']+" | head -n1)
finalResponse=$(curl -s -D - -X GET \
	-H "Cookie: $cookie" \
	$finalUrl)

# FINALLY (Create new cookies with the values we actually want)
ls=$(echo "$finalResponse" | grep -i '^set-cookie: LS=' | sed -E 's/.*LS=([^;]+).*/\1/')
csrf=$(echo "$finalResponse" | grep -i '^set-cookie: _csrf=' | sed -E 's/.*_csrf=([^;]+).*/\1/')

cookie="LS=$ls; _csrf=$csrf"

# Create/Get Draft ID
draftResponse=$(curl -s -X POST \
	-H "Cookie: $cookie" \
	-H "X-Csrf-Token: $csrf" \
	-H "X-Source: publisher-portal" \
	-H "Content-Type: application/json" \
	-H "User-Agent: Mozilla/5.0" \
	-d "{\"path\":\"/management/draft/1227062\"}" \
	https://publisher.unity.com/publisher-v2-api/proxy)

draftId=$(echo $draftResponse | jq -r '.id')

# Get package details
detailsResponse=$(curl -s -X GET \
	-H "Cookie: $cookie" \
	-H "X-Csrf-Token: $csrf" \
	-H "X-Source: publisher-portal" \
	-H "Content-Type: application/json" \
	-H "User-Agent: Mozilla/5.0" \
	https://publisher.unity.com/publisher-v2-api/management/package-version/$draftId)

# Update package details
categoryId=$(echo $detailsResponse | jq '.package.version.category_id')
price=$(echo $detailsResponse | jq -r '.package.version.price')
versionName="\"v$newVersion\""
pTags=$(echo $detailsResponse | jq '.package.version.ptags')
language=$(echo $detailsResponse | jq '.package.version.languages | keys[]')
name=$(echo $detailsResponse | jq '.package.version.name')
elevatorPitch=$(echo $detailsResponse | jq '.package.version.languages | .[keys[0]].elevator_pitch')
keyFeatures=$(echo $detailsResponse | jq '.package.version.languages | .[keys[0]].key_features')
optionalDescription=$(echo $detailsResponse | jq '.package.version.languages | .[keys[0]].optional_description')
releaseNotes=$(echo $detailsResponse | jq '.package.version.languages | .[keys[0]].release_notes')
compatibilityInfo=$(echo $detailsResponse | jq '.package.version.languages | .[keys[0]].compatibility_info')

newJson="\
{\
	\"path\":\"/management/package-version/$draftId\", \
	\"data\":{ \
		\"category_id\":$categoryId, \
		\"price\":$price, \
		\"version_name\":$versionName, \
		\"portal_version\":\"v2\", \
		\"ptags\":$pTags, \
		\"metadata\":[ \
			{ \
				\"language\":$language, \
				\"name\":$name, \
				\"elevator_pitch\":$elevatorPitch, \
				\"key_features\":$keyFeatures, \
				\"optional_description\":$optionalDescription, \
				\"release_notes\":$releaseNotes, \
				\"compatibility_info\":$compatibilityInfo
			} \
		] \
	} \
}"

updateResponse=$(curl -s -X POST \
	-H "Cookie: $cookie" \
	-H "X-Csrf-Token: $csrf" \
	-H "X-Source: publisher-portal" \
	-H "Content-Type: application/json" \
	-H "User-Agent: Mozilla/5.0" \
	-d "$newJson" \
	https://publisher.unity.com/publisher-v2-api/proxy)

echo "$updateResponse"