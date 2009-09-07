﻿#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System.Security.Principal;
using System.Web;
using System.Web.Routing;
using Ninject;
using Ninject.Modules;
using Subtext.Configuration;
using Subtext.Framework;
using Subtext.Framework.Data;
using Subtext.Framework.Emoticons;
using Subtext.Framework.Providers;
using Subtext.Framework.Routing;
using Subtext.Framework.Services;
using Subtext.Framework.Syndication;
using Subtext.Framework.Web.HttpModules;
using Subtext.Infrastructure;
using Subtext.Framework.Configuration;

namespace Subtext
{
    public class Dependencies : NinjectModule
    {
        public override void Load()
        {
            // Main Services
            Bind<ITextTransformation>().ToMethod(context => {
                var transform = new CompositeTextTransformation();
                transform.Add(context.Kernel.Get<XhtmlConverter>());
                transform.Add(context.Kernel.Get<EmoticonsTransformation>());
                //TODO: Maybe use a INinjectParameter to control this.
                transform.Add(context.Kernel.Get<KeywordExpander>());
                return transform;
            } ).InRequestScope();
            Bind<ICommentService>().To<CommentService>().InRequestScope();
            Bind<ICommentFilter>().To<CommentFilter>().InRequestScope();
            Bind<IStatisticsService>().To<StatisticsService>().InRequestScope();
            Bind<ICommentSpamService>().To<AkismetSpamService>().InRequestScope()
                .WithConstructorArgument("apiKey", c => c.Kernel.Get<Blog>().FeedbackSpamServiceKey)
                .WithConstructorArgument("akismetClient", c => null);

            // Dependencies you're less likely to change.
            LoadCoreDependencies();
        }

        private void LoadCoreDependencies()
        {
            Bind<IEntryPublisher>().To<EntryPublisher>().InRequestScope();
            Bind<FriendlyUrlSettings>().ToMethod(context => FriendlyUrlSettings.Settings).InRequestScope();
            Bind<ISubtextPageBuilder>().To<SubtextPageBuilder>().InSingletonScope();
            Bind<ISlugGenerator>().To<SlugGenerator>().InRequestScope();
            Bind<FriendlyUrlSettings>().To<FriendlyUrlSettings>().InRequestScope();
            Bind<IPrincipal>().ToMethod(context => context.Kernel.Get<RequestContext>().HttpContext.User).InRequestScope();
            Bind<Blog>().ToMethod(c => BlogRequest.Current.Blog).When(r => BlogRequest.Current.Blog != null).InRequestScope();
            Bind<ObjectProvider>().ToMethod(c => new DatabaseObjectProvider()).InRequestScope();
            Bind<Subtext.Infrastructure.ICache>().To<SubtextCache>().InRequestScope();
            Bind<System.Web.Caching.Cache>().ToMethod(c => HttpContext.Current.Cache).InRequestScope();
            Bind<OpmlWriter>().To<OpmlWriter>().InRequestScope();
            Bind<IKernel>().ToMethod(context => context.Kernel).InSingletonScope();
            Bind<Tracking>().ToMethod(context => Config.Settings.Tracking).InSingletonScope();

            Bind<RouteCollection>().ToConstant(RouteTable.Routes).InSingletonScope();
            Bind<HttpContext>().ToMethod(c => HttpContext.Current).InRequestScope();
            Bind<ISubtextContext>().To<SubtextContext>().InRequestScope();
            Bind<RequestContext>().ToMethod(c => Bootstrapper.RequestContext).InRequestScope();
        }
    }
}