using Pooshit.Ocelot.Entities.Attributes;

namespace Pooshit.Ocelot.Tests.Entities; 

/// <summary>
/// campaign with jobs to publish at a job broker
/// </summary>
public class Campaign {

    /// <summary>
    /// id of campaign
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }

    /// <summary>
    /// name of campaign
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// description for campaign
    /// </summary>
    public string Description { get; set; }
        
    /// <summary>
    /// budget for the whole campaign
    /// </summary>
    /// <remarks>
    /// the campaign is only valid if the sum of all job budgets does not exceed the budget of the campaign
    /// </remarks>
    public decimal Budget { get; set; }

    /// <summary>
    /// maximum price per application paid by customer
    /// </summary>
    public decimal Cpa { get; set; }

    /// <summary>
    /// click conversion rate used to estimate number of applications
    /// if applications can not get measured
    /// </summary>
    [DefaultValue(0.0)]
    public decimal CCR { get; set; }
        
    /// <summary>
    /// status of campaign
    /// </summary>
    [Index("status")]
    public CampaignStatus Status { get; set; }

    /// <summary>
    /// predicate for jobs of sources to match
    /// </summary>
    public string Predicate { get; set; }
        
    /// <summary>
    /// determines whether to use mamgo landing pages
    /// </summary>
    [DefaultValue(0)]
    public bool UseMamgoLandingPages { get; set; }

    /// <summary>
    /// origin linked to this campaign
    /// used to reduce error margin when syncing events
    /// </summary>
    [Index("origin")]
    public string Origin { get; set; }

    /// <summary>
    /// determines whether campaign items are posted to job ufo
    /// </summary>
    [Index("jobufo")]
    [DefaultValue(0)]
    public bool UseJobUfo { get; set; }

    /// <summary>
    /// uses publisher as company for landing pages
    /// </summary>
    [DefaultValue(0)]
    public bool UsePublisher { get; set; }
        
    /// <summary>
    /// pattern to use when generating job url
    /// </summary>
    public string ApplyUrlPattern { get; set; }

    /// <summary>
    /// script used for modifying routing logic
    /// </summary>
    public string RoutingScript { get; set; }
}