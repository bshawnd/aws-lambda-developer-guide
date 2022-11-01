using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Util;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
// using Amazon.XRay.Recorder.Core;
// using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Newtonsoft.Json;
// using Newtonsoft.Json.Serialization;
using Datadog.Trace;
using Datadog.Trace.Configuration;
using Datadog.Trace.Annotations;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace blankCsharp
{
  public class Function
  {
    private static AmazonLambdaClient lambdaClient;

    static Function() {
      // // Create a settings object using the existing
      // // environment variables and config sources
      // var settings = TracerSettings.FromDefaultSources();
      // // Override a value
      // settings.GlobalTags.Add("SomeKey", "SomeValue");
      // settings.TraceEnabled = true;
      // settings.ServiceName = "net6-test";
      // settings.Environment = "dev";
      // settings.ServiceVersion = "0.0.1";
      // settings.LogsInjectionEnabled = false;
      // settings.TracerMetricsEnabled = true;
      // // Replace the tracer configuration
      // Tracer.Configure(settings);

      initialize();
    }

    static async void initialize() {
      // AWSSDKHandler.RegisterXRayForAllServices();
      lambdaClient = new AmazonLambdaClient();
      await callLambda();
    }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
    public async Task<AccountUsage> FunctionHandler(SQSEvent invocationEvent, ILambdaContext context)
    {
      using (IScope scope = Tracer.Instance.StartActive("custom-operation"))
      {
        var userDetails = new UserDetails()
        {
            // the systems internal identifier for the users
            Id = "d41452f2-483d-4082-8728-171a3570e930",
            // the email address of the user
            Email = "test@adventure-works.com",
            // the user's name, as displayed by the system
            Name = "Jane Doh",
            // the user's session id
            SessionId = "d0632156-132b-4baa-95b2-a492c5f9cb16",
            // the role the user is making the request under
            Role = "standard",
        };
        Tracer.Instance.ActiveScope?.Span.SetUser(userDetails);

        GetAccountSettingsResponse accountSettings;
        try
        {
          accountSettings = await callLambda();
        }
        catch (AmazonLambdaException ex)
        {
          throw ex;
        }
        AccountUsage accountUsage = accountSettings.AccountUsage;
        LambdaLogger.Log("ENVIRONMENT VARIABLES: " + JsonConvert.SerializeObject(System.Environment.GetEnvironmentVariables()));
        LambdaLogger.Log("CONTEXT: " + JsonConvert.SerializeObject(context));
        LambdaLogger.Log("EVENT: " + JsonConvert.SerializeObject(invocationEvent));

        return accountUsage;
      }
    }

    public static async Task<GetAccountSettingsResponse> callLambda()
    {
      var request = new GetAccountSettingsRequest();
      var response = await lambdaClient.GetAccountSettingsAsync(request);
      return response;
    }
  }
}
