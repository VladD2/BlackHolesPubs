using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackHoles.Models
{
  public class Comment
  {
    public int ArticleId { get; set; }
    public int ParentMsgId { get; set; }
    public string Text { get; set; }

    public override string ToString()
    {
      return $"ArticleId={ArticleId} ParentMsgId={ParentMsgId} {Text}";
    }
  }
}