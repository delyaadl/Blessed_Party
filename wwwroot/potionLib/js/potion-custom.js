var dropdown = document.getElementsByClassName("dropdown-btn");
var i;

for (i = 0; i < dropdown.length; i++) {
    dropdown[i].addEventListener("click", function () {
        this.classList.toggle("active");
        var dropdownContent = this.nextElementSibling;
        if (dropdownContent.style.display === "block") {
            dropdownContent.style.display = "none";
        } else {
            dropdownContent.style.display = "block";
        }
    });
}

window.onload = function () {
    copyrightYear();
};
function copyrightYear() {
    var d = new Date();
    var n = d.getFullYear();
    document.getElementById("copyrightYear").innerHTML = n;
}

$(document).ready(function () {
    var url = window.location;
    $('.sidebar .navbar-nav').find('.active').removeClass('active');
    $('.nav-link').each(function () {
        if (this.href === url.href) {
            $(this).parent().addClass('active');
        }
    });
    $('.dropdown-item-cust').each(function () {
        if (this.href === url.href) {
            $(this).parent().parent().addClass('active');
        }
    });

    $('#loadingScreen').css('display', 'none');

    var divsToHide = document.getElementsByClassName("bd-clipboard"); //divsToHide is an array
    for (var i = 0; i < divsToHide.length; i++) {
        divsToHide[i].style.visibility = "hidden"; // or
        divsToHide[i].style.display = "none"; // depending on what you're doing
    }

    $('[data-toggle="tooltip"]').tooltip();
    $('[data-toggle="popover"]').popover();

    $('#dataTable').DataTable();
});