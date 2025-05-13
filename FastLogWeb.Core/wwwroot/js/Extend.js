$.extend({
    Checkbox: {
        CheckAll: function (obj, checkboxName) {
            var checkBox = document.getElementsByName(checkboxName);
            for (var i = 0; i < checkBox.length; i++) { checkBox[i].checked = obj.checked; }
        },
        GetCheckedText: function (name) {
            var s = [];
            $("input[name=" + name + "]:checked").each(function () {
                s.push($(this).attr("text"));
            });
            return s.join(",");
        },
        GetCheckedValue: function (name) {
            var s = [];
            $("input[name=" + name + "]:checked").each(function () {
                s.push($(this).val());
            });
            return s.join(",");
        },
        IsCheked: function (name) {
            var isSelect = false;
            $("input[name=" + name + "]:checked").each(function () {
                isSelect = true;
            });
            return isSelect;
        },
        GetValue: function (name) {
            var s = [];
            $("input[name=" + name + "]").each(function () {
                s.push($(this).attr("value"));
            });
            return s.join(",");
        },
    },
    TableClickColor: function (id) {
        $("#" + id + " tr").first().click();
        $("#" + id + "  tbody tr").click(function () {
            $(this).css("background-color", "#6CC2CC");
            $("#" + id + " tbody tr:odd").not(this).css("background-color", "#ffffff");
            $("#" + id + " tbody tr:even").not(this).css("background-color", "#f3f4f5");
        });
    }
});