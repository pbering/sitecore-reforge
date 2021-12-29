using Sitecore.Data.Items;
using Sitecore.Links.UrlBuilders;

namespace Reforge.BlackBox.Support
{
    public class MediaUrlBuilderWithPortSupport : MediaUrlBuilder
    {
        public MediaUrlBuilderWithPortSupport(DefaultMediaUrlBuilderOptions defaultOptions, string mediaLinkPrefix) : base(defaultOptions, mediaLinkPrefix)
        {

        }

        public override string Build(MediaItem item, MediaUrlBuilderOptions options)
        {
            if (string.IsNullOrEmpty(options.MediaLinkServerUrl))
            {
                var context = System.Web.HttpContext.Current;
                var scheme = context.Request.Headers[Sitecore.Configuration.Settings.LoadBalancingScheme];
                var host = context.Request.Headers[Sitecore.Configuration.Settings.LoadBalancingHost];

                if (!string.IsNullOrEmpty(scheme) && !string.IsNullOrEmpty(host))
                {
                    // Sitecore handles building the correct url when running behind reverse proxy
                    options.MediaLinkServerUrl = Sitecore.Web.WebUtil.GetServerUrl();
                }
                else
                {
                    // the HOST header contains both hostname+port when running in Docker container with exposed ports
                    options.MediaLinkServerUrl = $"http://{context.Request.Headers["HOST"]}";
                }
            }

            return base.Build(item, options);
        }
    }
}