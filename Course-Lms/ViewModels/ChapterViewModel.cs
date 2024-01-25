using Course_Lms.Logic.DTO;

namespace Course_Lms.ViewModels
{
	public class ChapterViewModel
	{
		public int ProgressCount { get; set; } = 0;
		public IEnumerable<SideBarChaptersListDTO> ChaptersList { get; set; }
		public ChapterViewDTO ChapterData { get; set; }

	}
}
