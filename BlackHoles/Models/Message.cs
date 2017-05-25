using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackHoles.Models
{
  public class Message
  {
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Поле должно быть заполнено!")]
    public DateTime Created { get; set; }


    [Display(Name = "Дата"), ForeignKey("Writer")]
    public string WriterId { get; set; }

    [Required(ErrorMessage = "Поле должно быть заполнено!")]
    public ApplicationUser Writer { get; set; }

    [Display(Name = "Текст сообщения"), Required(ErrorMessage = "Сообщение должно быть задано!"), DataType(DataType.MultilineText), StringLength(8000, MinimumLength = 3)]
    public string Text { get; set; }

    [Display(Name = "Ответы")]
    public List<Message> Messages { get; set; }

    public override string ToString()
    {
      return $"Id={Id}; Writer={Writer.Email} Text={Text}";
    }
  }
}
