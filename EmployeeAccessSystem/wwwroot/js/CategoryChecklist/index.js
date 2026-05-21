$(document).ready(function () {

    showCategoryChecklistToasts();

    initializeCategoryChecklistPagination();

});

// Show success/error messages
function showCategoryChecklistToasts() {

    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right',
        timeOut: '3000'
    };

    var successMessage = $('#tempSuccessMessage').val();
    var errorMessage = $('#tempErrorMessage').val();

    if (successMessage !== '') {
        toastr.success(successMessage, 'Success');
    }

    if (errorMessage !== '') {
        toastr.error(errorMessage, 'Message');
    }
}

// Initialize pagination and search
function initializeCategoryChecklistPagination() {

    if ($('#categoryChecklistTableBody tr.group-row').length === 0) {
        return;
    }

    var rowsPerPage = parseInt($('#entriesPerPage').val());
    var currentPage = 1;

    function getFilteredRows() {

        var searchText = $('#customSearch').val().toLowerCase().trim();
        var filteredRows = [];

        $('#categoryChecklistTableBody tr.group-row').each(function () {

            var rowSearchText = ($(this).attr('data-search') || '').toLowerCase();

            if (searchText === '' || rowSearchText.indexOf(searchText) > -1) {
                filteredRows.push(this);
            }

        });

        return filteredRows;
    }

    function updateSerialNumbers(filteredRows) {

        for (var i = 0; i < filteredRows.length; i++) {
            $(filteredRows[i]).find('.group-sn-wrap').text(i + 1);
        }
    }

    function renderPagination() {

        var filteredRows = getFilteredRows();
        var totalRows = filteredRows.length;
        var totalPages = Math.ceil(totalRows / rowsPerPage);

        $('#categoryChecklistTableBody tr.group-row').hide();

        if (totalRows === 0) {
            $('#paginationInfo').text('No matching entries found.');
            $('#customPagination').html('');
            return;
        }

        updateSerialNumbers(filteredRows);

        if (currentPage > totalPages) {
            currentPage = 1;
        }

        var startIndex = (currentPage - 1) * rowsPerPage;
        var endIndex = startIndex + rowsPerPage;

        for (var i = startIndex; i < endIndex && i < totalRows; i++) {
            $(filteredRows[i]).show();
        }

        var fromRecord = startIndex + 1;
        var toRecord = endIndex;

        if (toRecord > totalRows) {
            toRecord = totalRows;
        }

        $('#paginationInfo').text('Showing ' + fromRecord + ' to ' + toRecord + ' of ' + totalRows + ' grouped entries');

        var paginationHtml = '';

        if (currentPage > 1) {
            paginationHtml += '<li class="page-item"><a href="#" class="page-link pagination-link" data-page="' + (currentPage - 1) + '">Previous</a></li>';
        }
        else {
            paginationHtml += '<li class="page-item disabled"><span class="page-link">Previous</span></li>';
        }

        for (var page = 1; page <= totalPages; page++) {

            if (page === currentPage) {
                paginationHtml += '<li class="page-item active"><span class="page-link">' + page + '</span></li>';
            }
            else {
                paginationHtml += '<li class="page-item"><a href="#" class="page-link pagination-link" data-page="' + page + '">' + page + '</a></li>';
            }
        }

        if (currentPage < totalPages) {
            paginationHtml += '<li class="page-item"><a href="#" class="page-link pagination-link" data-page="' + (currentPage + 1) + '">Next</a></li>';
        }
        else {
            paginationHtml += '<li class="page-item disabled"><span class="page-link">Next</span></li>';
        }

        $('#customPagination').html(paginationHtml);
    }

    $('#customSearch').on('keyup', function () {

        currentPage = 1;

        renderPagination();

    });

    $('#entriesPerPage').on('change', function () {

        rowsPerPage = parseInt($(this).val());

        currentPage = 1;

        renderPagination();

    });

    $(document).on('click', '.pagination-link', function (event) {

        event.preventDefault();

        currentPage = parseInt($(this).data('page'));

        renderPagination();

    });

    renderPagination();
}