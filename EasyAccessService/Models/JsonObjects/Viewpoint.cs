// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using IssueConnectorWebService.Models.EasyAccess;
//
//    var data = ViewPoint.FromJson(jsonString);

namespace EasyAccessService.Models
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class ViewPoint
    {
        [JsonProperty("commentGuid")]
        public string CommentGuid { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("topicGuid")]
        public string TopicGuid { get; set; }

        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("date")]
        public System.DateTime Date { get; set; }

        [JsonProperty("viewPointImage")]
        public ViewPointImage ViewPointImage { get; set; }

        [JsonProperty("orthogonalCamera")]
        public OrthogonalCamera OrthogonalCamera { get; set; }

        [JsonProperty("perspectiveCamera")]
        public PerspectiveCamera PerspectiveCamera { get; set; }

        [JsonProperty("components")]
        public object Components { get; set; }

        [JsonProperty("lines")]
        public object Lines { get; set; }

        [JsonProperty("clippingPlanes")]
        public object ClippingPlanes { get; set; }

        [JsonProperty("importGuid")]
        public object ImportGuid { get; set; }
    }

    public partial class OrthogonalCamera
    {
        [JsonProperty("cameraViewPoint")]
        public OrthogonalCameraCameraDirection CameraViewPoint { get; set; }

        [JsonProperty("cameraDirection")]
        public OrthogonalCameraCameraDirection CameraDirection { get; set; }

        [JsonProperty("cameraUpVector")]
        public OrthogonalCameraCameraDirection CameraUpVector { get; set; }

        [JsonProperty("viewToWorldScale")]
        public long ViewToWorldScale { get; set; }
    }

    public partial class OrthogonalCameraCameraDirection
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }

        [JsonProperty("z")]
        public long Z { get; set; }
    }

    public partial class PerspectiveCamera
    {
        [JsonProperty("cameraViewPoint")]
        public PerspectiveCameraCameraDirection CameraViewPoint { get; set; }

        [JsonProperty("cameraDirection")]
        public PerspectiveCameraCameraDirection CameraDirection { get; set; }

        [JsonProperty("cameraUpVector")]
        public PerspectiveCameraCameraDirection CameraUpVector { get; set; }

        [JsonProperty("fieldOfView")]
        public long FieldOfView { get; set; }
    }

    public partial class PerspectiveCameraCameraDirection
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }

    public partial class ViewPointImage
    {
        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public partial class ViewPoint
    {
        public static ViewPoint FromJson(string json) => JsonConvert.DeserializeObject<ViewPoint>(json, EasyAccessService.Models.Converter.Settings);
    }

    public static class SerializeViewpoint
    {
        public static string ToJson(this ViewPoint self) => JsonConvert.SerializeObject(self, EasyAccessService.Models.Converter.Settings);
    }

    public class ViewpointConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
