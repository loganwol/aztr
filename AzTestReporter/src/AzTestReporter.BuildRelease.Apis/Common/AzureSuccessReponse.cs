namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Validation;

    /// <summary>
    /// A generic representation of a standard collections response from Azure.
    /// </summary>
    public class AzureSuccessReponse
    {
        /// <summary>
        /// Helper to convert a Json string to Azure object.
        /// </summary>
        /// <param name="responseBody">Azure response body.</param>
        /// <returns>A object representation of the JSON.</returns>
        public static AzureSuccessReponse ConverttoAzureSuccessResponse(string responseBody)
        {
            JObject returnData = (JObject)JsonConvert.DeserializeObject(responseBody);
            return new AzureSuccessReponse()
            {
                Count = Convert.ToInt32(((JValue)returnData["count"]).Value.ToString()),
                Value = returnData["value"],
            };
        }

        public static AzureSuccessReponse BuildAzureSuccessResponseFromValueArray(string valuearraystring, int count = -1)
        {
            JObject returnData = (JObject)JsonConvert.DeserializeObject("{ 'value' : " + valuearraystring + "}");
            return new AzureSuccessReponse()
            {
                Count = count == -1? returnData["value"].Count(): count,
                Value = returnData["value"],
            };
        }

        /// <summary>
        /// Gets or sets the count of objects in the response.
        /// </summary>
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the collection of objects in the response.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public JToken Value { get; set; }

        /// <summary>
        /// A generic helper to Convert Azure responses to a collection of objects of T.
        /// </summary>
        /// <typeparam name="T">A type.</typeparam>
        /// <param name="asr">Input Azure success response.</param>
        /// <returns>A list of objects of T.</returns>
        public static List<T> ConvertTo<T>(AzureSuccessReponse asr)
        {
            Requires.NotNull(asr, nameof(asr));

            List<T> dataList = new List<T>();
            for (int i = 0; i < asr.Count; i++)
            {
                dataList.Add(JsonConvert.DeserializeObject<T>(asr.Value[i].ToString()));
            }

            return dataList;
        }

        public static List<T> ConvertTo<T>(string json)
        {
            Requires.NotNullOrEmpty(json, nameof(json));

            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(json);
            return AzureSuccessReponse.ConvertTo<T>(successResponse);
        }
    }
}
