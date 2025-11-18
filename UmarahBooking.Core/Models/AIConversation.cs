using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manisik.Models
{
	public class AIConversation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]

		public int AIConversationId { get; set; }

		// FK to ApplicationUser (Guid)
		[Required]
		public int ?UserId { get; set; }

		[Required]
		[MaxLength(100)]
		public string ?SessionId { get; set; } = string.Empty;

		// Stores conversation messages in JSON format
		[Required]
		public string ?ConversationHistoryJson { get; set; } = "{}";

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[Required]
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

		[ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
	}
}