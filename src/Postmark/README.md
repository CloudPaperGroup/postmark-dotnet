# Postmark .NET (CloudPaperGroup fork)

> **This is not an official Postmark package.** It is CloudPaperGroup's fork of
> [ActiveCampaign/postmark-dotnet](https://github.com/ActiveCampaign/postmark-dotnet),
> published as `CloudPaperGroup.PostmarkDotNet`. The API, the namespaces and the
> assembly name are the same as the official `Postmark` package, so you can switch
> between the two by changing only the package reference, as long as your code does not
> use the two settings below (they exist only in this fork). Do not reference both
> packages: they contain the same `Postmark.dll`.
>
> **What the fork adds:** the client's JSON serializer options can be replaced
> (`PostmarkJsonOptions.Response` and `PostmarkJsonOptions.Request`),
> and the defaults work in apps that turn reflection-based System.Text.Json off
> (`JsonSerializerIsReflectionEnabledByDefault=false`).

The .NET client for [Postmark](https://postmarkapp.com). Postmark helps
you deliver and track transactional and broadcast email for your applications,
replacing SMTP with a reliable, scalable HTTP API. This package wraps the
Postmark API so you can send email, manage templates, read bounces, configure
webhooks, and more without building your own HTTP calls.

Get a free API token at https://postmarkapp.com.

## Installation

```
dotnet add package CloudPaperGroup.PostmarkDotNet --version 1.0.0
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

### 5.4.1

- Added missing webhook payload models for type-safe deserialization of incoming
  webhook requests:
  - `PostmarkClickWebhookMessage` — click tracking webhook.
  - `PostmarkSpamComplaintWebhookMessage` — spam complaint webhook.
  - `PostmarkSubscriptionChangeWebhookMessage` — subscription change webhook.

  These join the existing `PostmarkBounceWebhookMessage`, `PostmarkDeliveryWebhookMessage`,
  `PostmarkOpenWebhookMessage`, and `PostmarkInboundWebhookMessage`.

### 5.4.0

- Added Bulk Email API support: `SendBulkEmailAsync` (`POST /email/bulk`) and
  `GetBulkEmailStatusAsync` (`GET /email/bulk/{id}`) for broadcast/marketing sends.

## Links

- [Source & issues](https://github.com/ActiveCampaign/postmark-dotnet)
- [Documentation wiki](https://github.com/ActiveCampaign/postmark-dotnet/wiki)
- [Postmark API reference](https://postmarkapp.com/developer)

## License

Licensed under the [MIT](https://github.com/ActiveCampaign/postmark-dotnet/blob/main/LICENSE) license.
