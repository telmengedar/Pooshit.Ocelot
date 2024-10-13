using System;
using System.Runtime.Serialization;
using Pooshit.Ocelot.Entities.Attributes;

namespace NightlyCode.Database.Tests.Models {
    
    /// <summary>
    /// click data of neuvoo
    /// </summary>
    public class NeuvooClick {
        
        /// <summary>
        /// id of click
        /// </summary>
        [PrimaryKey]
        [DataMember(Name="click_id")]
        public string ClickId { get; set; }
        
        /// <summary>
        /// timestamp of click
        /// </summary>
        [Index("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// date string in EST timezone
        /// </summary>
        [DataMember(Name="dateEST")]
        public DateTime DateEST { get; set; }

        /// <summary>
        /// ip which clicked
        /// </summary>
        [DataMember(Name="ip")]
        public string IP { get; set; }

        /// <summary>
        /// id of job in neuvoo
        /// </summary>
        [DataMember(Name="job_id")]
        public string JobID { get; set; }

        /// <summary>
        /// job title
        /// </summary>
        [DataMember(Name = "job_title")]
        public string JobTitle { get; set; }

        /// <summary>
        /// seems to be some employer code
        /// </summary>
        [DataMember(Name = "empcode")]
        public string EmpCode { get; set; }

        /// <summary>
        /// name of publishing company
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// cost of click
        /// </summary>
        [DataMember(Name = "billed_ppc")]
        public decimal? Cost { get; set; }

        /// <summary>
        /// currency of payment
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// redirection url
        /// </summary>
        [DataMember(Name = "redirect_to")]
        public string RedirectTo { get; set; }

        /// <summary>
        /// job id of client
        /// </summary>
        [Index("jobid")]
        [DataMember(Name = "client_job_id")]
        public string ClientJobId { get; set; }
        
        /// <summary>
        /// id of campaign
        /// </summary>
        [DefaultValue(0)]
        [Index("campaign")]
        public long CampaignId { get; set; }
        
        /// <summary>
        /// id of item
        /// </summary>
        [DefaultValue(0)]
        [Index("item")]
        public long ItemId { get; set; }

        /// <summary>
        /// id of target item
        /// </summary>
        [Index("target")]
        [DefaultValue(0)]
        public long TargetId { get; set; }

        /// <summary>
        /// id of job
        /// </summary>
        [DefaultValue(0)]
        [Index("job")]
        public long MamgoJobId { get; set; }

        /// <summary>
        /// cpc for which click was registered
        /// </summary>
        public decimal? Cpc { get; set; }
    }}