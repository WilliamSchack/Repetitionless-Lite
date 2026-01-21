import sys
import UnityPublisherAPI as UnityAPI

# Inputs
unityEmail = sys.argv[1]
unityPass = sys.argv[2]

newVersionTag = sys.argv[3]
releaseNotesHtmlFile = sys.argv[4]

# Read html file
releaseNotesHtml = open(releaseNotesHtmlFile, "r").read()
releaseNotesHtml = releaseNotesHtml.replace("\n", "")

# Update package details
print("Authorising...")
session = UnityAPI.Authorisation.GetAuthenticatedSession(unityEmail, unityPass)
print("Successfully Authorised!")

print("Creating/Getting Draft...")
packageId = UnityAPI.Packages.GetPackageId(session, "Repetitionless")
draftId = UnityAPI.Packages.GetDraftId(session, packageId)

print("Updating Draft...")
UnityAPI.Packages.UpdateDraft(session, draftId, {
    "version_name": newVersionTag,
    "metadata": [
        {
            "release_notes": releaseNotesHtml
        }
    ]
})

# print("Submitting")
# UnityAPI.Packages.SubmitDraft(session, draftId, False, "")