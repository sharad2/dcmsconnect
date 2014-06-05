﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Net.Mail;

namespace DcmsMobile.Models
{
    public class ConnectionString
    {
        public string Name { get; set; }

        public string DataSource { get; set; }

        public string ProxyUserId { get; set; }

        /// <summary>
        /// For debugging only
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}@{2}", this.Name, this.ProxyUserId, this.DataSource);
        }
    }

    public class DiagnosticModel : ViewModelBase
    {
        public char Sound { get; set; }

        public IEnumerable<ConnectionString> ConnectionStrings
        {
            get
            {
                return from ConnectionStringSettings settings in ConfigurationManager.ConnectionStrings
                       let builder = new DbConnectionStringBuilder
                       {
                           ConnectionString = (settings.ConnectionString)
                       }
                       select new ConnectionString
                       {
                           Name = settings.Name,
                           DataSource = (builder["Data Source"] ?? "").ToString(),
                           ProxyUserId = (builder["Proxy User Id"] ?? "").ToString()
                       };
            }
        }
    }

    public class DiagnosticEmailModel : ViewModelBase
    {
        public DiagnosticEmailModel()
        {
            this.Subject = "Test e-mail from DcmsMobile Diagnostics";
            this.Body = "This is a sample e-mail generated by DcmsMobile. Please do not reply to this e-mail.";
            using (var smtp = new SmtpClient())
            {
                // Show smtp.Host
                this.HostName = smtp.Host;
                var mail = new MailMessage();
                // Show From address available in mail
                this.From = mail.From.Address;
            }
        }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "E-mail Address")]
        public string To { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Body Text")]
        public string Body { get; set; }

        [Display(Name = "SMTP Server")]
        public string HostName { get; set; }

        public string From { get; set; }
    }
}




//<!--$Id$-->