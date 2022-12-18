﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    /* PRODUCTS SLIDER */
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
    $('.bp-rating').starRating({
        starSize: 25,
        strokeWidth: 0,
        activeColor: '#ffa700',
        readOnly: true,
        useGradient: false,
    });

    /* SHOW MORE SHOW LESS */
    $('.bp-description').readall({
        showheight: 100,
        btnTextShowmore: 'Показати повністю',
        btnTextShowless: 'Показати менше',
        animationspeed: 200
    });

    /* BOOK PAGE ADD TO WISH LIST ACTIONS */
    $('#wishBtnContainer').on('click', function () {
        var clickedEl = $(this).children('button');

        var id = clickedEl.attr('id');

        if (id == 'wishAddBtn') {
            var reqURL = '/WishList/Add';
            var newText = 'Видалити зі списку бажань'
            var newFill = 'red';
            var newId = 'wishRmBtn';
        }
        else if (id == 'wishRmBtn') {
            var reqURL = '/WishList/Remove';
            var newText = 'Додати до списку бажань'
            var newFill = 'none';
            var newId = 'wishAddBtn';
        }
        else return;

        var bookId = clickedEl.attr('data-bookId');
        if (typeof bookId === 'undefined' || bookId === '') {
            alert("Сталася помилка");
            return;
        }
        $.ajax({
            type: 'POST',
            url: reqURL,
            dataType: 'json',
            data: { 'bookId': bookId },
            success: function (response) {
                if (response.success === true) {
                    clickedEl.find('span').text(newText);
                    clickedEl.find('svg path:first').css('fill', newFill);
                    clickedEl.attr('id', newId)
                }
                else alert("Sorry, error occured");
            },
            error: function (xhr, status, err) {
                if (xhr.status == 401) {
                    window.location.href = '/Account/Login';
                }
                else {
                    alert("Failed");
                }
            }
        });
    });

    /* BOOK CARD WISH LIST ACTIONS */
    $('main').on('click', '.s-item-btns', function (e) {
        var clickedEl = $(e.target);

        if (clickedEl.hasClass('cardWishAdd')) {
            var reqURL = '/WishList/Add';
            var newText = 'Видалити'
            var newClass = 'cardWishRm';
            var oldClass = 'cardWishAdd';
        }
        else if (clickedEl.hasClass('cardWishRm')) {
            var reqURL = '/WishList/Remove';
            var newText = 'Бажаю'
            var newClass = 'cardWishAdd';
            var oldClass = 'cardWishRm';
        }
        else return;

        var bookId = clickedEl.attr('data-bookId');
        if (typeof bookId === 'undefined' || bookId === '') {
            alert("Сталася помилка");
            return;
        }
        $.ajax({
            type: 'POST',
            url: reqURL,
            dataType: 'json',
            data: { 'bookId': bookId },
            success: function (response) {
                if (response.success === true) {
                    $('.cardWishListAction').find(`input[data-bookId=${bookId}]`).each(function () {
                        $(this).attr('value', newText);
                        $(this).addClass(newClass).removeClass(oldClass);
                    })
                }
                else alert("Sorry, error occured");
            },
            error: function (xhr, status, err) {
                if (xhr.status == 401) {
                    window.location.href = '/Account/Login';
                }
                else {
                    alert("Failed");
                }
            }
        });
    });

});


/*
 * jQuery.ReadAll ia a jQuery plugin to shrink large blocks of content and place a read more button below.
 * Created by Anders Fjällström - anders@morriz.net - http://www.morriz.net
 * For documentation see https://github.com/morriznet/jquery.readall
 * Released under MIT license
 * version 1.1
 */

(function ($) {
    $.fn.readall = function (options) {
        var settings = $.extend({
            // Default values
            showheight: 96,                         // height to show
            showrows: null,                         // rows to show (overrides showheight)
            animationspeed: 200,                    // speed of transition

            btnTextShowmore: 'Read more',           // text shown on button to show more
            btnTextShowless: 'Read less',           // text shown on button to show less
            btnClassShowmore: 'readall-button',     // class(es) on button to show more
            btnClassShowless: 'readall-button'      // class(es) on button to show less

        }, options);
        $(this).each(function () {
            var $this = $(this),
                fullheight = function () { return $this[0].scrollHeight; },
                wrapperclass = 'readall-wrapper',
                hiddenclass = 'readall-hide';
            if (settings.showrows != null) {
                var lineHeight = Math.floor(parseFloat($this.css('font-size')) * 1.5);
                settings.showheight = lineHeight * settings.showrows;
            }
            $this.addClass('readall').css({ 'overflow': 'hidden' });

            var onResize = function (event) {
                // on resize check if readall is needed
                var _button = $this.parent().find('button.' + settings.btnClassShowmore.replace(/\s+/g, '.') + ', button.' + settings.btnClassShowless.replace(/\s+/g, '.'));
                if (fullheight() > settings.showheight + $(_button).outerHeight()) {
                    if (!$(_button).is(':visible') || event == null) {
                        $this.css({ 'height': settings.showheight + 'px', 'max-height': settings.showheight + 'px' });
                        $(_button).text(settings.btnTextShowmore);
                        $this.addClass(hiddenclass);
                        $(_button).removeClass(settings.btnClassShowless).addClass(settings.btnClassShowmore);
                        $(_button).show();
                    }
                } else {
                    if ($(_button).is(':visible') || event == null) {
                        $this.css({ 'height': '', 'max-height': '' });
                        $this.removeClass(hiddenclass);
                        $(_button).hide();
                    }
                }
            };

            if ($this.parent().not(wrapperclass)) {
                $this.wrap($('<div />').addClass(wrapperclass));
                var _button = $('<button />').addClass(settings.btnClassShowmore).text(settings.btnTextShowmore).on('click', function (e) {
                    e.preventDefault();
                    if ($this.hasClass(hiddenclass)) {
                        $this.css({ 'height': settings.showheight + 'px', 'max-height': '' }).animate({ height: fullheight() + 'px' }, settings.animationspeed, function () {
                            $this.css({ 'height': '' });
                            $(_button).text(settings.btnTextShowless);
                        });
                    } else {
                        $this.animate({ 'height': settings.showheight + 'px' }, settings.animationspeed, function () {
                            $this.css({ 'max-height': settings.showheight + 'px' });
                            $(_button).text(settings.btnTextShowmore);
                        });
                    }
                    $this.toggleClass(hiddenclass);
                    $(this).toggleClass(settings.btnClassShowmore).toggleClass(settings.btnClassShowless);
                });
                $this.after(_button);

                $(window).on('orientationchange resize', onResize);

                onResize(null);
            }
        });
        return this;
    };
}(jQuery));
