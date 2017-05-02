var authorIds = [];
var url = "/Articles/AddAuthorsAjax";//"<not set>";

function attachEvents()
{
  $('#availableAuthorsComboBox').on("change", onChange);
  $('#authors').on("click", "a", onRemoveAuthor);
}

function updateAuthors(authorIds)
{
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

$(attachEvents);
