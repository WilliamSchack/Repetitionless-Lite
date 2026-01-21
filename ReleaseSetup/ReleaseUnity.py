import os
import sys
import UnityPublisherAPI as UnityAPI

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

# Inputs
unityEmail = sys.argv[1]
unityPass = sys.argv[2]

newVersionTag = sys.argv[3]
releaseNotesHtmlFile = sys.argv[4]

twoFactorCode = ""
if (len(sys.argv) >= 5): twoFactorCode = sys.argv[5]

# Read html file
releaseNotesHtml = open(os.path.join(__location__, releaseNotesHtmlFile), "r").read()
releaseNotesHtml = releaseNotesHtml.replace("\n", "")

# Update package details
print("Authorising...")
session = UnityAPI.Authorisation.GetAuthenticatedSession(unityEmail, unityPass, twoFactorCode)
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