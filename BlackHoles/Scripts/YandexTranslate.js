function onTranslate(from, to)
{
  var text = $('#' + from).val();

  var link = 'https://translate.yandex.net/api/v1.5/tr.json/translate?key=trnsl.1.1.20170511T234904Z.3cea83a2432cfe03.096f00d41d65407999dea5be124a39c5c08fa188&text='
    + text + '&lang=ru-en';

  $.ajax({
    type: 'GET',
    url: link,
    success: function (response) { $('#' + to).val(response.text[0]); }
  });

  return false;
}