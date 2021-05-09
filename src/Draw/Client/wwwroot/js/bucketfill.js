var BucketFill = BucketFill || {};
BucketFill.fill = function (startX, startY, fillColorR, fillColorG, fillColorB) {
    //console.log("startX: " + startX + ", startY: " + startY);
    //console.log("R: " + fillColorR + ", G: " + fillColorG + ", B: " + fillColorB);

    var canvasContainer = document.getElementById('canvas-wrapper'),
        canvas = canvasContainer.getElementsByTagName('canvas')[0];

    var context = canvas.getContext("2d");
    var canvasWidth = canvas.width;
    var canvasHeight = canvas.height;

    var drawingBoundTop = 0;

    var colorLayer = context.getImageData(0, 0, canvasWidth, canvasHeight);
    pixelStack = [[startX, startY]];

    startPixelPos = (startY * canvasWidth + startX) * 4;
    var startR = colorLayer.data[startPixelPos];
    var startG = colorLayer.data[startPixelPos + 1];
    var startB = colorLayer.data[startPixelPos + 2];
    var startA = colorLayer.data[startPixelPos + 3];

    if (startR == fillColorR && startG == fillColorG && startB == fillColorB && startA == 255) {
        return;
    }

    while (pixelStack.length) {
        var newPos, x, y, pixelPos, reachLeft, reachRight;
        newPos = pixelStack.pop();
        x = newPos[0];
        y = newPos[1];

        pixelPos = (y * canvasWidth + x) * 4;
        while (y-- >= drawingBoundTop && matchStartColor(pixelPos)) {
            pixelPos -= canvasWidth * 4;
        }
        pixelPos += canvasWidth * 4;
        ++y;
        reachLeft = false;
        reachRight = false;
        while (y++ < canvasHeight - 1 && matchStartColor(pixelPos)) {
            colorPixel(pixelPos);

            if (x > 0) {
                if (matchStartColor(pixelPos - 4)) {
                    if (!reachLeft) {
                        pixelStack.push([x - 1, y]);
                        reachLeft = true;
                    }
                }
                else if (reachLeft) {
                    reachLeft = false;
                }
            }

            if (x < canvasWidth - 1) {
                if (matchStartColor(pixelPos + 4)) {
                    if (!reachRight) {
                        pixelStack.push([x + 1, y]);
                        reachRight = true;
                    }
                }
                else if (reachRight) {
                    reachRight = false;
                }
            }

            pixelPos += canvasWidth * 4;
        }
    }
    context.putImageData(colorLayer, 0, 0);

    function matchStartColor(pixelPos) {
        var r = colorLayer.data[pixelPos];
        var g = colorLayer.data[pixelPos + 1];
        var b = colorLayer.data[pixelPos + 2];
        var a = colorLayer.data[pixelPos + 3];

        var dr = Math.abs(r - startR);
        var dg = Math.abs(g - startG);
        var db = Math.abs(b - startB);
        var da = Math.abs(a - startA);

        //return (r == startR && g == startG && b == startB && && a == startA);
        return (dr + dg + db + da) < 50;
    }

    function colorPixel(pixelPos) {
        colorLayer.data[pixelPos] = fillColorR;
        colorLayer.data[pixelPos + 1] = fillColorG;
        colorLayer.data[pixelPos + 2] = fillColorB;
        colorLayer.data[pixelPos + 3] = 255;
    }

};