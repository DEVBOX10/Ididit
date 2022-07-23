﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Ididit.Data;
using Ididit.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ididit.WebView.Persistence;

public class GoogleDriveBackup : IGoogleDriveBackup
{
    public async Task<DataModel> ImportData()
    {
        // https://developers.google.com/drive/api/guides/manage-downloads#.net

        return null;
    }

    public void ExportData(IDataModel data)
    {
        // https://stackoverflow.com/questions/60774277/check-if-folder-exist-in-google-drive-by-folder-name-c

        // https://stackoverflow.com/questions/47136576/google-drive-api-v3-c-net-searching-folders-files-by-title-throws-requesterr

        // https://stackoverflow.com/questions/72733152/google-drive-api-search-by-file-name-only-finds-the-file-if-it-has-recently-be

        // https://developers.google.com/drive/api/guides/folder#.net

        // https://developers.google.com/drive/api/guides/manage-uploads#.net
    }

    /* Global instance of the scopes required by this quickstart. If modifying these scopes, delete your previously saved token.json/ folder. */
    static string[] Scopes = { DriveService.Scope.DriveReadonly };
    static string ApplicationName = "Drive API .NET Quickstart";

    static void Main()
    {
        try
        {
            UserCredential credential;
            // Load client secrets.
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                /* The file token.json stores the user's access and refresh tokens, and is created automatically when the authorization flow completes for the first time. */
                string credPath = "token.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;

                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

            Console.WriteLine("Files:");

            if (files == null || files.Count == 0)
            {
                Console.WriteLine("No files found.");
                return;
            }

            foreach (var file in files)
            {
                Console.WriteLine("{0} ({1})", file.Name, file.Id);
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
