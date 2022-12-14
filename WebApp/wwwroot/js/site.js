// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    var slider = $('#autoWidth').lightSlider({
        autoWidth: true,
        loop: false,
        controls: false,
        slideMargin: 10,
        pager: false,
        onSliderLoad: function () {
            $('#autoWidth').removeClass('hidden');
        }
    });

    $('#goToPrevSlide').click(function () {
        slider.goToPrevSlide();
    });

    $('#goToNextSlide').click(function () {
        slider.goToNextSlide();
    });


    $(".bp-rating").starRating({
        starSize: 25,
        strokeWidth: 0,
        activeColor: '#ffa700',
        readOnly: true,
        useGradient: false,
    });
});
