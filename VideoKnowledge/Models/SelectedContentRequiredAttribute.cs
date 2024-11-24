using System.ComponentModel.DataAnnotations;

namespace VideoKnowledge.Models
{
    public class SelectedContentRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (VideoKnowledgeEvent)validationContext.ObjectInstance;
            if (model.EventType!=null && model.EventType == "0")
            {
                return new ValidationResult("You must select an event type to proceed.");
            }
            //image
            else if(model.EventType!=null && model.EventType=="1" && model.EvntImageUpload==null )
            {
                //ModelState.AddModelError("", "The Video Knowledge Event time cannot exceed " +
                //            "the duration of the  main video: " +
                //            vdkToUpdate.VideoDuration + " seconds");
                return new ValidationResult("Image file to upload is required.");
            }
            //video
            else if (model.EventType != null && model.EventType == "4" && model.EvntVideoUpload == null)
            {
                return new ValidationResult("Video file to upload is required.");
            }
            //Quiz
            else if (model.EventType != null && model.EventType == "2" && model.EvntQuizName == null)
            {
                return new ValidationResult("Quiz name is required.");
            }
            //Web Content
            else if (model.EventType != null && model.EventType == "3" && model.EvntWebLink == null)
            {
                    return new ValidationResult("Content link is required.");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}
