local angle = 0
local rectSize = 100

function setup()
    print("Hello from Lua!")
end

function update(dt)
    angle = angle + 90 * dt
end

function draw()
    background(30, 30, 40)

    fill(100, 200, 255)
    noStroke()
    pushMatrix()
    translate(width() / 2, height() / 2)
    rotate(angle)
    rect(-rectSize / 2, -rectSize / 2, rectSize, rectSize)
    popMatrix()

    fill(255, 100, 120)
    circle(mouseX(), mouseY(), 20)

    fill(255, 255, 255)
    textSize(16)
    text("SpriteCore - Hello Sprite!", 10, 25)
    text("Move mouse to interact", 10, 50)
    text("Press ESC to exit", 10, 75)
end
