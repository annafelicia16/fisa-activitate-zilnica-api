using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.DailyActivities.Models;

// A file stored on local disk for one daily activity record. FileName is the
// (sanitized) original name shown to users; StoredFileName is the server-
// generated on-disk name ("{Id}{ext}") so nothing user-controlled ever touches
// a path. Attachments are never rendered into the PDF export.
public class DailyActivityRecordAttachment
{
    [Key]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("DailyActivityRecordId")]
    public required string DailyActivityRecordId { get; set; }

    [Required]
    [Column("FileName")]
    public required string FileName { get; set; }

    [Required]
    [Column("StoredFileName")]
    public required string StoredFileName { get; set; }

    [Required]
    [Column("ContentType")]
    public required string ContentType { get; set; }

    [Required]
    [Column("SizeBytes")]
    public required long SizeBytes { get; set; }

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
