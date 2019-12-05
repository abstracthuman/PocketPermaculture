'use strict';

module.exports = function () {
    var $rating = $('.star-rating');
    $rating.on('click', 'a', function (e) {
        e.preventDefault();
        var starData = $(this).data('star');
        $(this).parent()
            .removeClass(function (index, className) {
                return className.match(/(active-rating-(\d))/g);
            })
            .addClass('active-rating-' + starData);
    });
};
