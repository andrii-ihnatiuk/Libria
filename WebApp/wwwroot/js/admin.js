$(document).ready(function () {

    $("#confirmDeleteModal").on("show.bs.modal", function (event) {
        var button = event.relatedTarget;

        var removeId = button.getAttribute("data-remove-id");
        var title = button.getAttribute("data-modal-title");

        $("#confirmDeleteModal").find(".modal-title").text(title);
        $("#modal-remove-id").attr("value", removeId);
    });

    $(".chosen-select").chosen({
        disable_search_threshold: 10,
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