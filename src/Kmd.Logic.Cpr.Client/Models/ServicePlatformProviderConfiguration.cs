// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Kmd.Logic.Cpr.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class ServicePlatformProviderConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the
        /// ServicePlatformProviderConfiguration class.
        /// </summary>
        public ServicePlatformProviderConfiguration()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// ServicePlatformProviderConfiguration class.
        /// </summary>
        /// <param name="environment">Possible values include: 'Production',
        /// 'Test'</param>
        public ServicePlatformProviderConfiguration(System.Guid? id = default(System.Guid?), System.Guid? subscriptionId = default(System.Guid?), string name = default(string), string certificateFileName = default(string), string environment = default(string), string municipalityCvr = default(string))
        {
            Id = id;
            SubscriptionId = subscriptionId;
            Name = name;
            CertificateFileName = certificateFileName;
            Environment = environment;
            MunicipalityCvr = municipalityCvr;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public System.Guid? Id { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionId")]
        public System.Guid? SubscriptionId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "certificateFileName")]
        public string CertificateFileName { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Production', 'Test'
        /// </summary>
        [JsonProperty(PropertyName = "environment")]
        public string Environment { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "municipalityCvr")]
        public string MunicipalityCvr { get; set; }

    }
}
