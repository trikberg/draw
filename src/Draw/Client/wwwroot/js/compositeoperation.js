var CompositeOperation = CompositeOperation || {};
CompositeOperation.set = function (operation) {
    var canvasContainer = document.getElementById('canvas-wrapper'),
        canvas = canvasContainer.getElementsByTagName('canvas')[0];

    var context = canvas.getContext("2d");

    context.globalCompositeOperation = operation;
};