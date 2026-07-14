using AutoWork.Domain.Common;

namespace AutoWork.Domain.Entities;

/// <summary>Join entity: Post &lt;-&gt; Hashtag.</summary>
public class PostHashtag : BaseEntity
{
    public Guid PostId { get; set; }

    public Guid HashtagId { get; set; }

    public Post Post { get; set; } = null!;

    public Hashtag Hashtag { get; set; } = null!;
}
