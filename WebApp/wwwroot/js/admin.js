$(document).ready(function () {

    $("#confirmDeleteModal").on("show.bs.modal", function (event) {
        var button = event.relatedTarget;

        var categoryId = button.getAttribute("data-category-id");
        var categoryName = button.getAttribute("data-category-name");

        $("#confirmDeleteModal").find(".modal-title").text(categoryName);
        $("#modal-category-id").attr("value", categoryId);
    });
})