function setup()
    print("Shapes Demo loaded!")
end

function draw()
    background(20, 20, 30)
    fill(255)
    textSize(20)
    text("SpriteCore Shapes Reference", 20, 30)

    local y = 60
    local spacing = 50

    fill(100, 200, 255)
    noStroke()
    rect(20, y, 40, 30)
    fill(255)
    textSize(14)
    text("rect(x, y, w, h)", 70, y + 20)
    y = y + spacing

    fill(255, 100, 120)
    ellipse(40, y + 15, 50, 30)
    fill(255)
    text("ellipse(x, y, w, h)", 70, y + 20)
    y = y + spacing

    fill(100, 255, 150)
    circle(40, y + 15, 30)
    fill(255)
    text("circle(x, y, r)", 70, y + 20)
    y = y + spacing

    stroke(255, 255, 100)
    strokeWeight(3)
    line(20, y + 15, 60, y + 15)
    fill(255)
    noStroke()
    text("line(x1, y1, x2, y2)", 70, y + 20)
    y = y + spacing

    fill(200, 100, 255)
    triangle(20, y + 30, 40, y, 60, y + 30)
    fill(255)
    text("triangle(x1,y1, x2,y2, x3,y3)", 70, y + 20)
    y = y + spacing

    noFill()
    stroke(255, 200, 100)
    strokeWeight(3)
    circle(40, y + 15, 40)
    fill(255)
    noStroke()
    text("circle (stroke demo)", 70, y + 20)

    -- 右侧列：变换演示
    local x2 = 400
    y = 60

    fill(255)
    textSize(14)
    text("Transforms:", x2, y)
    y = y + 30

    pushMatrix()
    translate(x2 + 40, y + 40)
    rotate(millis() / 10)
    fill(255, 100, 100)
    rect(-20, -20, 40, 40)
    popMatrix()
    fill(255)
    text("rotate + translate", x2 + 80, y + 40)

    -- 底部：数学函数演示
    y = 400
    fill(255)
    textSize(14)
    text("Math Demo: noise + lerp", 20, y)

    for i = 0, 300, 2 do
        local nx = i / 50
        local ny = millis() / 1000
        local n = noise(nx, ny)
        local h = map(n, -1, 1, 10, 60)
        fill(lerp(100, 255, n), lerp(200, 100, n), 255)
        rect(20 + i, y + 20, 2, h)
    end
end
