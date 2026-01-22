import os
import sys
import ItchPublisherAPI as ItchAPI

__location__ = os.path.realpath(
    os.path.join(os.getcwd(), os.path.dirname(__file__)))

# Inputs
itchEmail = sys.argv[1]
itchPass = sys.argv[2]
itchTwoFactor = sys.argv[3]

newVersion = f"v{sys.argv[4]}"
releaseNotesHtmlFile = sys.argv[5]

# Read html file
releaseNotesHtml = open(os.path.join(__location__, releaseNotesHtmlFile), "r").read()
releaseNotesHtml = releaseNotesHtml.replace("\n", "")

# Create new devlog post
session = ItchAPI.Authorisation.GetAuthenticatedSession(itchEmail, itchPass, itchTwoFactor)
packageId = ItchAPI.Packages.GetPackageId(session, "Repetitionless")

lastFileId = ItchAPI.Packages.GetLastUploadedFileId(session, packageId)
ItchAPI.Packages.PublishNewDevlog(session, packageId, newVersion, releaseNotesHtml, [ lastFileId ])