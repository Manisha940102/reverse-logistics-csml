namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("olist_order_reviews")]
public class OlistOrderReview
{
    [Column("review_id")]
    [StringLength(50)]
    public string ReviewId { get; set; } = string.Empty;

    [Column("order_id")]
    [StringLength(50)]
    public string OrderId { get; set; } = string.Empty;

    [Column("review_score")]
    public int ReviewScore { get; set; }

    [Column("review_comment_title")]
    [StringLength(200)]
    public string? ReviewCommentTitle { get; set; }

    [Column("review_comment_title_english")]
    [StringLength(200)]
    public string? ReviewCommentTitleEnglish { get; set; }

    [Column("review_comment_message")]
    public string? ReviewCommentMessage { get; set; }

    [Column("review_comment_message_english")]
    public string? ReviewCommentMessageEnglish { get; set; }

    [Column("review_creation_date")]
    public DateTime? ReviewCreationDate { get; set; }

    [Column("review_answer_timestamp")]
    public DateTime? ReviewAnswerTimestamp { get; set; }

    // Navigation
    [ForeignKey("OrderId")]
    public OlistOrder? Order { get; set; }
}
