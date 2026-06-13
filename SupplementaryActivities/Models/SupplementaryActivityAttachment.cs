using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FisaActivitateZilnicaApi.SupplementaryActivities.Models;

// A file stored on local disk for one supplementary activity — mirrors
// DailyActivityRecordAttachment. FileName is the (sanitized) original name;
// StoredFileName is the server-generated on-disk name ("{Id}{ext}").
// Attachments are never rendered into the PDF export.
public class SupplementaryActivityAttachment
{
    [Key]
    [Column("Id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("SupplementaryActivityId")]
    public required string SupplementaryActivityId { get; set; }

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
