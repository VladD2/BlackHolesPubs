﻿var authorIds = [];

function attachEvents()
{
  var value = "[" + $('#AuthorsIds').val() + "]";
  authorIds = eval(value);

  $('#availableAuthorsComboBox').on("change", onChange);
  $('#authors').on("click", "a", onRemoveAuthor);

  // extend jquery range validator to work for required checkboxes
  var defaultRangeValidator = $.validator.methods.range;
  $.validator.methods.range = validatorRange;
}

function validatorRange(value, element, param)
{
  if (element.type === 'checkbox')
    return element.checked; // if it's a checkbox return true if it is checked
  else
    return defaultRangeValidator.call(this, value, element, param); // otherwise run the default validation function
}

function updateAuthors(authorIds)
{
  $('#AuthorsIds').val(authorIds);
  $.ajax({
    traditional: true,
    url: urlPrefix + "Articles/AddAuthorsAjax",
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

  var authorIds2 = jQuery.grep(authorIds,
    function (item) {
      return item != authorId;
    });

  authorIds.filter(function (item) { return item != authorId; });
  if (authorIds2.length == authorIds.length)
    return false;
  authorIds = authorIds2;
  updateAuthors(authorIds2);

  console.log("<<< onRemoveAuthor");
  return false;
}

function onSubmit()
{
  if (!window.File)
    return true;

  var files = document.getElementById("articleFile").files;

  if (files.length == 0)
    return true;

  var file = files[0];

  if (file.lastModifiedDate === 'undefined')
    return true;
  $('#ArticleDate').val(file.lastModifiedDate.toJSON());

  return true;
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
    url: urlPrefix + "Articles/UploadArticle",
    type: "POST",
    data: formData,
    contentType: false,
    processData: false,
    success: function () {
      alert("URA");
    }
  });
}

function addComment(articleId, parentMsgId)
{
  var areaName = parentMsgId > 0 ? "replyText_" + parentMsgId : "CurrentMessageText";
  var textArea = document.getElementById(areaName);
  var text = textArea.value;
  var data = { ArticleId: articleId, Text: text };

  if (parentMsgId > 0)
    data.ParentMsgId = parentMsgId;

  execCommentAjax(data, 'AddCommentAjax', 'добавить', function () { textArea.value = ''; });

  return false;
}

function deleteComment(articleId, msgId)
{
  if (confirm('Вы уверены, что хотите удалить этот комментарий?'))
    execCommentAjax({ articleId: articleId, msgId: msgId }, 'DeletCommentAjax', 'удалить', undefined);

  return false;
}

function execCommentAjax(data, action, actionTitle, onSuccess)
{
  $.ajax({
    traditional: true,
    url: urlPrefix + "Articles/" + action,
    type: "POST",
    contentType: "application/json; charset=utf-8",
    data: JSON.stringify(data),
    success: function (data) {
      $('#comments').replaceWith(data);
      if (onSuccess != undefined)
        onSuccess();
    },
    error: function (XMLHttpRequest, textStatus, errorThrown) {
      alert('Не удается ' + actionTitle + ' комментарий!');
    }
  });
}

function onIssueChanged(issueText)
{
  var issue  = eval(issueText);
  var yeat   = issue[0];
  var number = issue[1];
  $('#IssueYear').val(yeat);
  $('#IssueNumber').val(number);
}

$(attachEvents);
