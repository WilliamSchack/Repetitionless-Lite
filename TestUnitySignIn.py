import sys
import re
import requests

# Functions
def RecursiveMerge(dict1, dict2): #https://www.geeksforgeeks.org/python/recursively-merge-dictionaries-in-python/
    for key, value in dict2.items():
        if key in dict1 and isinstance(dict1[key], dict) and isinstance(value, dict):
            # Recursively merge nested dictionaries
            RecursiveMerge(dict1[key], value)
        else:
            # Merge non-dictionary values
            dict1[key] = value

def RequestWithMaxRedirects(requestFunc):
    try:
        return requestFunc()
    except requests.exceptions.TooManyRedirects as e:
        return e.response

def AuthoriseSession(session: requests.Session):
    # Start Conversation
    # Set-Cookie: conversationId

    session.max_redirects = 2
    conversationStarterResponse = RequestWithMaxRedirects(
        lambda: session.get("https://publisher.unity.com")
    )

    # Get csrf Token
    # Set-Cookie: __Host-authjs.csrf-token

    csrfResponse = session.get(
        "https://login.unity.com/api/auth/csrf",
        allow_redirects = False
    )

    csrfToken = csrfResponse.json()["csrfToken"]

    # Get Session Token
    # Set-Cookie: __Secure-authjs.session-token

    credentialsResponse = session.post(
        "https://login.unity.com/api/auth/callback/credentials",
        allow_redirects = False,
        headers = {
            "Content-Type": "application/x-www-form-urlencoded",
        },
        data = {
            "email": unityEmail,
            "password": unityPass,
            "csrfToken": csrfToken,
            "callbackUrl": "https://login.unity.com/en/sign-in"
        }
    )

    # Authorise
    # Set-Cookie: _hashed_user_id

    session.max_redirects = 1
    authoriseResponse = RequestWithMaxRedirects(
        lambda: session.get("https://login.unity.com/en/sign-in")
    )

    # Get the final cookies
    # Set-Cookie: LS
    # Set-Cookie: _csrf

    finalUrl = re.search(r'url=([^"\']+)', authoriseResponse.content.decode("utf-8")).group(1)
    finalResponse = session.get(finalUrl)

    # Set Required Headers
    session.headers.update({
        "X-Csrf-Token": session.cookies.get_dict()["_csrf"],
        "X-Source": "publisher-portal"
    })

def GetPackageId(session: requests.Session, packageName: str) -> int:
    response = session.post(
        "https://publisher.unity.com/publisher-v2-api/management/packages",
        json = {
            "limit": "100",
            "order": "asc",
            "order_by": "name"
        }
    )

    for package in response.json()["package_versions"]:
        if package["name"] != packageName:
            continue

        return package["id"]
    
    return -1

# Creates a draft if one doesnt exist
def GetDraftId(session: requests.Session, packageId: str) -> str:
    response = session.post(
        "https://publisher.unity.com/publisher-v2-api/proxy",
        json = {
            "path": f"/management/draft/{packageId}"
        }
    )

    return response.json()["id"]

def GetPackageDetails(session: requests.Session, packageId: str) -> dict:
    response = session.get(f"https://publisher.unity.com/publisher-v2-api/management/package-version/{packageId}")
    return response.json()["package"]

def UpdateDraft(session: requests.Session, draftId: str, details: dict):
    packageDetails = GetPackageDetails(session, draftId)
    packageDetails = packageDetails["version"]

    language = list(packageDetails["languages"].keys())[0]
    languageDetails = packageDetails["languages"][language]

    # Convert package details to draft update format
    draftDetails = {
        "category_id": packageDetails["category_id"],
        "price": packageDetails["price"],
        "version_name": packageDetails["version_name"],
        "portal_version": packageDetails["portal_version"],
        "ptags": packageDetails["ptags"],
        "metadata": [
            {
                "language": language,
                "name": languageDetails["name"],
                "elevator_pitch": languageDetails["elevator_pitch"],
                "key_features": languageDetails["key_features"],
                "optional_description": languageDetails["optional_description"],
                "release_notes": languageDetails["release_notes"],
                "compatibility_info": languageDetails["compatibility_info"]
            }
        ]
    }

    # Insert new details
    RecursiveMerge(draftDetails, details)

    # Update draft
    response = session.post(
        "https://publisher.unity.com/publisher-v2-api/proxy",
        json = {
            "path": f"/management/package-version/{packageDetails["id"]}",
            "data": draftDetails
        }
    )

    return 0

def DeleteDraft(session: requests.Session, draftId: str):
    response = session.post(
        "https://publisher.unity.com/publisher-v2-api/proxy",
        json = {
            "method": "DELETE",
            "path": f"/management/draft/{draftId}"
        }
    )

# Untested
def SubmitDraft(session: requests.Session, draftId: str, autoPublish: bool, submitMessage: str):
    response = session.post(
        "https://publisher.unity.com/publisher-v2-api/proxy",
        json = {
            "path": f"/management/submit/{draftId}",
            "data": {
                "auto_publish": ("Y" if autoPublish else "N"),
                "submit_message": submitMessage,
                "termsaccept": "3"
            }
        }
    )

# Inputs
unityEmail = sys.argv[1]
unityPass = sys.argv[2]

newVersion = sys.argv[3]

# Setup Session
session = requests.Session()
session.headers.update({
    "User-Agent": "Mozilla/5.0",
    "Accept": "*/*",
    "Accept-Encoding": "gzip, deflate, br, zstd",
    "Accept-Language": "en-US,en;q=0.9",
    "Content-Type": "application/json",
    "Connection": "keep-alive"
})

print("Authorising...")
AuthoriseSession(session)
print("Successfully Authorised")

# Do Stuff
packageId = GetPackageId(session, "Repetitionless")
draftId = GetDraftId(session, packageId)

UpdateDraft(session, draftId, {
    "version_name": newVersion
})