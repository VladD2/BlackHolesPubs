  $(function () {
    $('[data-date]').each(function () {
      // the date construct will automatically convert to local time
      var localDate = new Date(parseInt($(this).attr('data-date')));

      $(this).html(localDate.toLocaleString());
    });
  });
