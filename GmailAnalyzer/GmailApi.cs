using System;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

namespace GmailAnalyzer
{
    class GmailApi
    {
        private const string CredPath = "token.json";
        
        private const string ApplicationName = "Gmail API .NET Quickstart";

        private const string AuthenticatedEmail = "me";
        
        private readonly GmailService _service;

        public GmailApi(string clientId, string clientSecretId, string[] scopes, string applicationName = ApplicationName)
        {
            _service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecretId
                    },
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CredPath, true)).Result,
                ApplicationName = applicationName,
            });
        }
        
        public GmailApi(string clientId, string clientSecretId, string applicationName = ApplicationName)
        {
            _service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecretId
                    },
                    new []{GmailService.Scope.GmailReadonly, GmailService.Scope.GmailSend},
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CredPath, true)).Result,
                ApplicationName = applicationName,
            });
        }

        private static string CreateRawEmailPayload(string toEmail, string fromEmail, string subject, string body)
        {
            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(fromEmail),
                To = { toEmail },
                ReplyToList = { fromEmail },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            return MimeKit.MimeMessage.CreateFromMailMessage(mailMessage).ToString();
        }

        public void SendEmail(string toEmail, string fromEmail, string subject, string body, string userId = AuthenticatedEmail)
        {
            _service.Users.Messages.Send(
                new Message {Raw = CreateRawEmailPayload(toEmail,fromEmail,subject,body)},
                userId);
        }

        public List<Message> GetEmailInboxByCustomQuery(string query, string userId = AuthenticatedEmail)
        {
            var filteredEmailList = new List<Message>();

            var request = _service.Users.Messages.List(userId);
            request.Q = (query);

            while (!String.IsNullOrEmpty(request.PageToken))
            {
                try
                {
                    var response = request.Execute();
                    filteredEmailList.AddRange(response.Messages);
                    request.PageToken = response.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                }
            }

            return filteredEmailList;
        }

        public List<Message> GetEmailBySender(string fromEmail)
        {
            return GetEmailInboxByCustomQuery("from:" + fromEmail);
        }
        
        public List<Message> GetEmailBySubject(string subject)
        {
            return GetEmailInboxByCustomQuery("subject:" + subject);
        }
        
        public void DeleteEmailById(string emailId, string userId = AuthenticatedEmail)
        {
            _service.Users.Messages.Trash(emailId, emailId).Execute();
        }

        public void DeleteMessageBySender(string fromEmail)
        {
            var filteredEmails = GetEmailBySender(fromEmail);
            filteredEmails.ForEach(email =>
            {
                DeleteEmailById(email.Id);
            });
        }
        
        public void DeleteMessageBySubject(string subject)
        {
            var filteredEmails = GetEmailBySubject(subject);
            filteredEmails.ForEach(email =>
            {
                DeleteEmailById(email.Id);
            });
        }
    }
}