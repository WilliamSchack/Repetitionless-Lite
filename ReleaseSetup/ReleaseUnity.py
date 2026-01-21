import sys
import UnityPublisherAPI as UnityAPI

# Inputs
unityEmail = sys.argv[1]
unityPass = sys.argv[2]

print("Authorising...")
session = UnityAPI.Authorisation.GetAuthenticatedSession(unityEmail, unityPass)
print("Successfully Authorised")

packageId = UnityAPI.Packages.GetPackageId(session, "Repetitionless")
draftId = UnityAPI.Packages.GetDraftId(session, packageId)

print(draftId)