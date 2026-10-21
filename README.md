# Postmark .NET (CloudPaperGroup fork)

> **This is not an official Postmark repository.** It is CloudPaperGroup's fork of
> [ActiveCampaign/postmark-dotnet](https://github.com/ActiveCampaign/postmark-dotnet) (MIT),
> published to NuGet as `CloudPaperGroup.PostmarkDotNet`. The API, namespaces and assembly
> name match the official `Postmark` package, so switching back needs only a package reference change,
> as long as your code does not use the two settings below. They exist only in this fork.
>
> **What the fork adds:** replaceable JSON serializer options
> (`PostmarkClientBase.ResponseSerializerOptions` and `RequestSerializerOptions`), with defaults
> that work in apps that turn reflection-based System.Text.Json off
> (`JsonSerializerIsReflectionEnabledByDefault=false`).
>
> **Releasing:** bump `<Version>` in `src/Postmark/Postmark.csproj` in your pull request. A merge to `main`
> publishes to the org's GitHub Packages feed. An Engineering team member then approves the `release`
> deployment to publish the same package to nuget.org.
>
> The rest of this file is the upstream README.

## Leave transactional email delivery to the experts

Postmark helps deliver and track transactional emails for web applications. In a nutshell, the service replaces SMTP (or Sendmail) with a far more reliable, scalable and care-free environment. In addition, you can track statistics for the number of emails sent, bounces and spam complaints.

The Postmark.NET NuGet package makes it easier to use the Postmark API from your projects without having to build your own API calls. You can get your free API token at http://www.postmarkapp.com

### Usage
Please see the [wiki](https://github.com/wildbit/postmark-dotnet/wiki) for instructions about sending email, using the bounce api, and additional sending options.

### Issues &amp; Comments
Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License
The Postmark .NET Library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/wildbit/postmark-dotnet/blob/master/LICENSE) file for more information.
