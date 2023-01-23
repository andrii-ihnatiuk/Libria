// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function loadSliders() {
    $('.slider').each(function () {
        let inst = $(this).lightSlider({
            autoWidth: true,
            loop: false,
            controls: false,
            slideMargin: 10,
            pager: false,
            onSliderLoad: function (el) {
                el.removeClass('hidden');
            }
        });
        $(this).closest(".slider-container").find(".slider-controls").data("controlInstance", inst);
    });
    $(".goToPrevSlide").click(function () {
        let slider = $(this).parent().data("controlInstance")
        slider.goToPrevSlide();
    });
    $('.goToNextSlide').click(function () {
        let slider = $(this).parent().data("controlInstance")
        slider.goToNextSlide();
    });
}

$(document).ready(function () {
    /* PRODUCTS SLIDER */
    loadSliders()

    // Star rating items
    $('.bp-rating').starRating({
        starSize: 25,
        strokeWidth: 0,
        activeColor: '#ffa700',
        readOnly: true,
        useGradient: false,
    });
    $('.review-rating').each(function () {
        $(this).starRating({
            starSize: 15,
            strokeWidth: 0,
            activeColor: '#ffa700',
            readOnly: true,
            useGradient: false,
        });
    });
    $('.review-create-rating').starRating({
        starSize: 35,
        strokeWidth: 0,
        totalStars: 5,
        minRating: 1,
        initialRating: 3,
        activeColor: "orange",
        ratedColor: "orange",
        readOnly: false,
        useFullStars: true,
        useGradient: false,
        disableAfterRate: false,
        callback: function (currentRating, $el) {
            let starsQuantity = $("#starsQuantity");
            if (starsQuantity === undefined) {
                alert("Щось пішло не так");
                return;
            }
            starsQuantity.val(currentRating);
        }
    });
    // Book page create review input
    $("#reviewText").on("input", function () {
        $("#reviewTextHelpBox span").text(`${1000 - $(this).val().length}`)
    })
    // Profile page collapse order details
    $(".order-collapse-toggle").each(function () {
        $(this).on("click", function () {
            let collapse = $(this).closest("tr").next().find(".collapse");
            if (collapse !== undefined)
                bootstrap.Collapse.getOrCreateInstance(collapse).toggle();
        })
    });
    // Profile page settings tab animations
    $(".info-toggle").on("click", function () {
        let container = $(this).closest(".info-container");
        container?.children(".info-item-wrapper").animate({
            opacity: 0,
            height: 0
        }, 250, function () {
            $(this).hide();
            let collapse = container.find(".collapse");
            if (collapse !== undefined)
                bootstrap.Collapse.getOrCreateInstance(collapse).show();
        });
    });
    $(".info-cancel").on("click", function () {
        let collapse = $(this).closest(".collapse");
        collapse.on("hidden.bs.collapse", function () {
            let wrapper = $(this).closest(".info-container").find(".info-item-wrapper");
            wrapper.css("display", "flex");
            wrapper.animate({
                opacity: 1,
                height: 48
            }, 250);
        });
        bootstrap.Collapse.getInstance(collapse).hide();
    });

    /* SHOW MORE SHOW LESS */
    $('.bp-description').readall({
        showheight: 100,
        btnTextShowmore: 'Показати повністю',
        btnTextShowless: 'Показати менше',
        animationspeed: 200
    });

    /* BOOK PAGE ADD TO WISH LIST  */
    $('#wishBtnContainer').on('click', function () {
        var clickedEl = $(this).children('button');
        clickedEl.attr('disabled', true);

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
        else {
            clickedEl.removeAttr('disabled');
            return;
        }

        var bookId = clickedEl.attr('data-bookId');
        if (typeof bookId === 'undefined' || bookId === '') {
            alert("Не знайдено ідентифікатор товару");
            clickedEl.removeAttr('disabled');
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
                else alert("Вибачте, сталася помилка");
            },
            error: function (xhr, status, err) {
                if (xhr.status == 401) {
                    window.location.href = '/Account/Login';
                }
                else {
                    alert('Невдалий запит: ' + err);
                }
            }
        }).always(function () {
            clickedEl.removeAttr('disabled');
        });
    });

    /* BOOK PAGE ADD TO CART */
    $('#cartAddBtn').on('click', function () {
        $(this).attr('disabled', true);
        var bookId = $(this).attr('data-bookId');
        if (typeof bookId === 'undefined' || bookId === '') {
            alert("Не знайдено ідентифікатор товару");
            $(this).removeAttr('disabled');
            return;
        }
        var thisBtn = $(this);
        $.ajax({
            type: 'POST',
            url: '/Cart/Add',
            dataType: 'json',
            data: { 'bookId': bookId },
            success: function (response) {
                if (response.success == true) {
                    alert(`Додано!\nУ кошику ${response.newQuantity} шт.\nВсього у кошику товарів на суму: ${response.totalCartPrice} грн.`);
                }
                else alert("Вибачте, сталася помилка");
            },
            error: function (xhr, status, err) {
                alert('Невдалий запит: ' + err);
            },
        }).always(function () {
            $(thisBtn).removeAttr('disabled');
        });
    });


    /* BOOK CARD ADD TO WISH LIST */
    $('main').on('click', '.cardWishListAction', function (e) {
        var clickedEl = $(e.target);
        if (clickedEl.data("disabled") === true)
            return;

        clickedEl.data("disabled", true);

        if (clickedEl.hasClass('cardWishAdd')) {
            var reqURL = '/WishList/Add';
            var newColor = 'red'
            var newClass = 'cardWishRm';
            var oldClass = 'cardWishAdd';
        }
        else if (clickedEl.hasClass('cardWishRm')) {
            var reqURL = '/WishList/Remove';
            var newColor = 'transparent'
            var newClass = 'cardWishAdd';
            var oldClass = 'cardWishRm';
        }
        else {
            clickedEl.data("disabled", false);
            return;
        }

        var bookId = clickedEl.attr('data-bookId');
        if (typeof bookId === 'undefined' || bookId === '') {
            alert("Не знайдено ідентифікатор товару");
            clickedEl.data("disabled", false);
            return;
        }
        $.ajax({
            type: 'POST',
            url: reqURL,
            dataType: 'json',
            data: { 'bookId': bookId },
            success: function (response) {
                if (response.success === true) {
                    $('.cardWishListAction').find(`div[data-bookId=${bookId}]`).each(function () {
                        $(this).addClass(newClass).removeClass(oldClass);
                        $(this).find("svg g path:first-child").css("fill", newColor);
                    })
                }
                else alert("Вибачте, сталася помилка");
            },
            error: function (xhr, status, err) {
                if (xhr.status == 401) {
                    window.location.href = '/Account/Login';
                }
                else {
                    alert('Невдалий запит: ' + err);
                }
            }
        }).always(function () {
            clickedEl.data("disabled", false);
        });
    });

    /* BOOK CARD ADD TO CART */
    $('main').on('click', '.cardCartAction', function (e) {
        var clickedEl = $(e.target);
        clickedEl.attr('disabled', true);

        var bookId = clickedEl.attr('data-bookId');
        if (typeof bookId === 'undefined' || bookId === '') {
            alert("Сталася помилка");
            clickedEl.removeAttr('disabled');
            return;
        }
        $.ajax({
            type: 'POST',
            url: '/Cart/Add',
            dataType: 'json',
            data: { 'bookId': bookId },
            success: function (response) {
                if (response.success === true) {
                    alert(`Додано!\nУ кошику ${response.newQuantity} шт.\nВсього у кошику товарів на суму: ${response.totalCartPrice} грн.`);
                }
                else alert("Вибачте, сталася помилка\n" + response.errorMessage);
            },
            error: function (xhr, status, err) {
                alert('Невдалий запит: ' + err);
            }
        }).always(function () {
            clickedEl.removeAttr('disabled');
        });
    });


    /* CART PAGE ADD MORE, REMOVE, FULL REMOVE */
    $('main').on('click', '.cart-item-actions', function (e) {
        var clickedEl = $(e.target);
        clickedEl.attr('disabled', true);
        var bookId = clickedEl.attr('data-bookId');

        if (clickedEl.hasClass('cartBtnRemove')) {
            var reqURL = '/Cart/Remove';
            var reqData = { 'bookId': bookId }; 
        }
        else if (clickedEl.hasClass('cartBtnFullRemove')) {
            var reqURL = '/Cart/Remove';
            var reqData = { 'bookId': bookId, 'fullRemove': true };
        }
        else if (clickedEl.hasClass('cartBtnAdd')) {
            var reqURL = '/Cart/Add';
            var reqData = { 'bookId': bookId };
        }
        else {
            clickedEl.removeAttr('disabled');
            return;
        }

        if (typeof bookId === 'undefined' || bookId === '') {
            alert("Сталася помилка");
            clickedEl.removeAttr('disabled');
            return;
        }

        var keepDisabled = false;

        $.ajax({
            type: 'POST',
            url: reqURL,
            dataType: 'json',
            data: reqData,
            success: function (response) {
                if (response.success === true) {
                    if (response.newQuantity === 0) {
                        // if full remove - delete element from DOM after request
                        clickedEl.closest('.cart-item').remove();
                    }
                    else if (response.newQuantity !== 0 && response.totalItemPrice !== undefined && response.totalCartPrice !== undefined) {
                        if (response.newQuantity === 1) {
                            // disable minus button if only 1 copy of item in cart
                            clickedEl.attr('disabled', true);
                            keepDisabled = true;
                        }
                        else {
                            clickedEl.siblings('.cartBtnRemove').attr('disabled', false);
                        }

                        clickedEl.siblings('span').text(response.newQuantity);
                        clickedEl.closest('.cart-item-actions').find('.totalItemPrice').text(response.totalItemPrice)
                    }
                    else {
                        alert("Сталася непередбачувана помилка");
                    }
                    $('.totalCartPrice').text(response.totalCartPrice);
                }
                else alert("Вибачте, сталася помилка\n" + response.errorMessage);
            },
            error: function (xhr, status, err) {
                alert('Невдалий запит: ' + err);
            }
        }).always(function () {
            if (keepDisabled === false) {
                clickedEl.removeAttr('disabled');
            }
        });
    });

    /* CART PAGE CLEAR CART */
    $('main').on('click', '.btn-clear-cart', function (e) {
        var clickedEl = $(e.target);
        clickedEl.attr('disabled', true);
        $.ajax({
            type: 'POST',
            url: '/Cart/Clear',
            dataType: 'json',
            success: function (response) {
                if (response.success === true) {
                    $('.cart-item').each(function () {
                        $(this).remove();
                    });
                    $('.totalCartPrice').text(response.totalCartPrice);
                }
                else alert("Вибачте, сталася помилка\n" + response.errorMessage);
            },
            error: function (xhr, status, err) {
                alert('Невдалий запит: ' + err);
            }
        }).always(function () {
            clickedEl.removeAttr('disabled');
        });
    });


    $("#subscribeNotificationModal").on("show.bs.modal", function () {
        modalEl = this;
        // get an instance of form falidator
        let validator = $("#subscribeNotificationForm").validate();
        if (validator !== undefined) {
            // override default submit behaviour
            validator.settings.submitHandler = function (form, event) {
                bootstrap.Modal.getInstance(modalEl).hide();

                let bookId = $(form).find("input[name='bookId']").val();
                let type = $(form).find("input[name='type']").val();
                let userEmail = $(form).find("input[name='userEmail']").val();
                if (bookId === undefined || type === undefined || userEmail === undefined) {
                    alert("Вибачте, щось пішло не так");
                }
                else {
                    $.ajax({
                        type: "POST",
                        url: "/Book/NotifyMe",
                        dataType: "json",
                        data: { "bookId": bookId, "userEmail": userEmail, "type": type },
                        success: function (response) {
                            let toastObj = $("#notificationToast");
                            // success
                            if (response.status === 0) {
                                toastObj.removeClass("bg-danger bg-secondary");
                                toastObj.addClass("bg-primary");
                                let about = type == "PriceDrop" ? "про зниження ціни" : "про наявність товару"
                                toastObj.find(".toast-body").text("Ми сповістимо вас " + about);
                            }
                            // already registered
                            else if (response.status === 2) {
                                toastObj.removeClass("bg-primary bg-danger");
                                toastObj.addClass("bg-secondary");
                                toastObj.find(".toast-body").text("Ви вже підписані на це сповіщення");
                            }
                            // failed
                            else {
                                toastObj.removeClass("bg-primary bg-secondary");
                                toastObj.addClass("bg-danger");
                                toastObj.find(".toast-body").text("Вибачте, щось пішло не так");
                            }
                            toastObj.toast("show");
                        }

                    });
                }
            }
        }
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
