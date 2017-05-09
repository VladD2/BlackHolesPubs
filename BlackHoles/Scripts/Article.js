var authorIds = [];
var url = "/Articles/AddAuthorsAjax";//"<not set>";

function attachEvents()
{
  var value = "[" + $('#AuthorsIds').val() + "]";
  authorIds = eval(value);

  $('#availableAuthorsComboBox').on("change", onChange);
  $('#authors').on("click", "a", onRemoveAuthor);
  $('#uploadArticle').on('click', onUploadArticle);
}

function updateAuthors(authorIds)
{
  $('#AuthorsIds').val(authorIds);
  $.ajax({
    traditional: true,
    url: url,
    type: "POST",
    contentType: "application/json; charset=utf-8",
    data: JSON.stringify(authorIds),
    success: function (data) {
      $('#authors').html(data);
      attachEvents();
    }
  });
}

function onChange()
{
  var authorId = this.value;
  authorIds.push(authorId);
  updateAuthors(authorIds);
  return false;
}

function onRemoveAuthor()
{
  console.log(">>> onRemoveAuthor");

  var authorId = $(this).data("id");
  if (authorId == null)
    return false;

  var authorIds2 = authorIds.filter(item => item != authorId);
  if (authorIds2.length == authorIds.length)
    return false;
  authorIds = authorIds2;
  updateAuthors(authorIds2);

  console.log("<<< onRemoveAuthor");
  return false;
}

function onUploadArticle(e)
{
  e.preventDefault();
  var formData = new FormData();

  var file = document.getElementById("articleFile").files[0];
  formData.append("articleFile", file);

  file = document.getElementById("additionalText").files[0];
  formData.append("additionalText", file);

  file = document.getElementById("additionalImg").files[0];
  formData.append("additionalImg", file);

  $.ajax({
    url: "/Articles/UploadArticle",
    type: "POST",
    data: formData,
    contentType: false,
    processData: false,
    success: function () {
      alert("URA");
    }
  });
}

function addComment(articleId)
{
  var val = document.getElementById("commentText_" + articleId).value;
  alert(val);
  return false;
}

function addReply(articleId, parentId)
{
  var val = document.getElementById("replyText_" + parentId).value;
  alert(val);
  return false;
}
$(attachEvents);
