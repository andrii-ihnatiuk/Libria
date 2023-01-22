$(document).ready(function () {

    $("#confirmDeleteModal").on("show.bs.modal", function (event) {
        var button = event.relatedTarget;

        var removeId = button.getAttribute("data-remove-id");
        var title = button.getAttribute("data-modal-title");

        $("#confirmDeleteModal").find(".modal-title").text(title);
        $("#modal-remove-id").attr("value", removeId);
    });

    $("#publisherEditModal").on("show.bs.modal", function (event) {
        let clicked = $(event.relatedTarget);

        let title = clicked.attr("data-modal-title");

        let modal = $("#publisherEditModal");
        modal.find(".modal-title").text(title);

        if (clicked.hasClass("add-publisher")) {
            modal.find("#publisherName").val("");
            modal.find("form").attr("action", "/Admin/Publishers/Create");
        }
        else if (clicked.hasClass("edit-publisher")) {
            let id = clicked.attr("data-edit-id");
            if (id === undefined) {
                alert("Не знайдено ідентифікатор видавництва");
                return;
            }
            modal.find("#publisherId").val(id);
            let name = clicked.attr("data-edit-name");
            modal.find("#publisherName").val(name);
            modal.find("form").attr("action", "/Admin/Publishers/Edit");
        }
    })

    $(".chosen-select-multi").chosen({
        disable_search_threshold: 10,
        no_results_text: "Упс, нічого не знайдено: ",
        width: "100%"
    });

    $(".chosen-select").chosen({
/*        disable_search_threshold: 10,*/
        no_results_text: "Упс, нічого не знайдено: ",
        width: "100%"
    });

    // Event to set image preview after user selected one
    $("#formFile").on("change", function () {
        let file = this.files[0];
        let reader = new FileReader();

        reader.readAsDataURL(file);
        reader.onload = function () {
            $("#imagePreview").attr("src", reader.result)
        }
        reader.onerror = function () {
            alert(`Помилка при читанні файлу.\nОпис: ${reader.error}`);
            $("#formFile").val(""); // reset file input
            return;
        }
    })

    $("#availableSwitch").on("change", function () {
        if ($(this).prop("checked") === true) {
            $("label.form-check-label").text("В наявності");
        }
        else {
            $("label.form-check-label").text("Немає в наявності");
        }
    })
})