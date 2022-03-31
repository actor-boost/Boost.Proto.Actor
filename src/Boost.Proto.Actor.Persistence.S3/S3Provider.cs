using Proto.Persistence;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;

namespace Boost.Proto.Actor.Persistence.S3;

public record S3Provider(string BucketName,
                         string AwsRegionEndpoint) : IProvider
{
    public Task DeleteEventsAsync(string actorName, long inclusiveToIndex) => throw new NotImplementedException();
    public Task DeleteSnapshotsAsync(string actorName, long inclusiveToIndex) => throw new NotImplementedException();
    public Task<long> GetEventsAsync(string actorName, long indexStart, long indexEnd, Action<object> callback) => throw new NotImplementedException();
    public Task<(object? Snapshot, long Index)> GetSnapshotAsync(string actorName) => throw new NotImplementedException();
    public Task<long> PersistEventAsync(string actorName, long index, object @event) => throw new NotImplementedException();
    public Task PersistSnapshotAsync(string actorName, long index, object snapshot)
    {
        var s3 = new AmazonS3Client();

        var dto = JsonSerializer.Serialize(snapshot);

        var body = @"{"
                 + @$"""N"":""{snapshot.GetType().Assembly.FullName}"","
                 + @$"""A"":""{snapshot.GetType().Assembly.FullName}"","
                 + @$"""T"":""{snapshot.GetType().Name}"","
                 + @$"""V"":{dto}"
                 + @"}";

        var ret = s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = BucketName,
            ContentBody = body
        });
    }
}
