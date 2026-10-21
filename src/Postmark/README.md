# Postmark .NET (CloudPaperGroup fork)

> **This is not an official Postmark package.** It is CloudPaperGroup's fork of
> [ActiveCampaign/postmark-dotnet](https://github.com/ActiveCampaign/postmark-dotnet),
> published as `CloudPaperGroup.PostmarkDotNet`. The API, the namespaces and the
> assembly name are the same as the official `Postmark` package, so you can switch
> between the two by changing only the package reference, as long as your code does not
> use the additions below (they exist only in this fork). Do not reference both
> packages: they contain the same `Postmark.dll`.
>
> **What the fork adds:** `PostmarkClient` and `PostmarkAdminClient` take optional JSON
> serializer options (`responseSerializerOptions` and `requestSerializerOptions`). Leave them
> out to use the defaults, which work in apps that turn reflection-based System.Text.Json off
> (`JsonSerializerIsReflectionEnabledByDefault=false`). To pass your own, start from
> `PostmarkJsonOptions.CreateDefaultResponse()` or `CreateDefaultRequest()`.
>
> The defaults cover only the calls that the client makes. If your app reads the webhook
> models (`PostmarkDotNet.Webhooks`) itself with reflection off, pass options that have a
> resolver, for example `PostmarkJsonOptions.CreateDefaultResponse()`.

The .NET client for [Postmark](https://postmarkapp.com). Postmark helps
you deliver and track transactional and broadcast email for your applications,
replacing SMTP with a reliable, scalable HTTP API. This package wraps the
Postmark API so you can send email, manage templates, read bounces, configure
webhooks, and more without building your own HTTP calls.

Get a free API token at https://postmarkapp.com.

## Installation

```
dotnet add package CloudPaperGroup.PostmarkDotNet
```

## Quick start

```csharp
using PostmarkDotNet;

var client = new PostmarkClient("your-server-token");

var message = new PostmarkMessage
{
    To = "recipient@example.com",
    From = "sender@example.com",
    Subject = "Hello from Postmark",
    TextBody = "This is a test message.",
    HtmlBody = "<strong>This is a test message.</strong>",
    MessageStream = "outbound"
};

var response = await client.SendMessageAsync(message);
```

See the [wiki](https://github.com/ActiveCampaign/postmark-dotnet/wiki) for guides on
sending email, using the bounce API, templates, and additional options.

## What's New

### 1.0.0 (CloudPaperGroup fork, based on upstream 5.4.1)

- The client constructors take optional `responseSerializerOptions` and `requestSerializerOptions`.
  The defaults set `DefaultJsonTypeInfoResolver`, so the client works in apps that turn
  reflection-based System.Text.Json off. You can pass your own options, for example with a
  source-generated resolver. `PostmarkJsonOptions` gives copies of the defaults to start from.

### 5.4.1 (upstream)

- Added missing webhook payload models for type-safe deserialization of incoming
  webhook requests:
  - `PostmarkClickWebhookMessage` — click tracking webhook.
  - `PostmarkSpamComplaintWebhookMessage` — spam complaint webhook.
  - `PostmarkSubscriptionChangeWebhookMessage` — subscription change webhook.

  These join the existing `PostmarkBounceWebhookMessage`, `PostmarkDeliveryWebhookMessage`,
  `PostmarkOpenWebhookMessage`, and `PostmarkInboundWebhookMessage`.

### 5.4.0 (upstream)

- Added Bulk Email API support: `SendBulkEmailAsync` (`POST /email/bulk`) and
  `GetBulkEmailStatusAsync` (`GET /email/bulk/{id}`) for broadcast/marketing sends.

## Links

- [Source & issues for this fork](https://github.com/CloudPaperGroup/postmark-dotnet)
- [Upstream source](https://github.com/ActiveCampaign/postmark-dotnet)
- [Documentation wiki](https://github.com/ActiveCampaign/postmark-dotnet/wiki)
- [Postmark API reference](https://postmarkapp.com/developer)

## License

Licensed under the [MIT](https://github.com/CloudPaperGroup/postmark-dotnet/blob/main/LICENSE) license.
