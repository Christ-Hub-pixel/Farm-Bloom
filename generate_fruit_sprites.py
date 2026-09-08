import os
from PIL import Image, ImageDraw

out_dir = r"f:/Farm Bloom/Assets/Art/Sprites"
os.makedirs(out_dir, exist_ok=True)
size = 256

# ==================== 1. STRAWBERRY 🍓 ====================
def draw_strawberry():
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Strawberry body (polygon smoothed or multi-circle)
    # Berry base: wide top (y=70), pointy bottom (y=225)
    points = [
        (128, 225),  # bottom tip
        (90, 205),
        (55, 160),
        (50, 110),
        (75, 75),
        (128, 85),  # top dip
        (181, 75),
        (206, 110),
        (201, 160),
        (166, 205),
    ]
    # Shadow/outline
    draw.polygon([(x, y + 3) for x, y in points], fill=(160, 20, 35, 255))
    # Main red berry
    draw.polygon(points, fill=(235, 35, 55, 255))

    # Rounded filler circles for smooth shape
    draw.ellipse([60, 80, 140, 160], fill=(235, 35, 55, 255))
    draw.ellipse([116, 80, 196, 160], fill=(235, 35, 55, 255))
    draw.ellipse([75, 120, 181, 215], fill=(235, 35, 55, 255))

    # Seeds (golden yellow specks)
    seeds = [
        (100, 110), (128, 115), (156, 110),
        (85, 140), (112, 145), (144, 145), (171, 140),
        (95, 175), (128, 175), (161, 175),
        (115, 200), (141, 200),
    ]
    for sx, sy in seeds:
        draw.ellipse([sx - 3, sy - 4, sx + 3, sy + 4], fill=(255, 225, 90, 255))

    # Specular bubble highlight (top-left)
    draw.ellipse([75, 95, 105, 125], fill=(255, 160, 170, 180))
    draw.ellipse([82, 100, 92, 110], fill=(255, 255, 255, 220))

    # Green leafy crown (calyx)
    leaf_c = (50, 180, 55, 255)
    leaf_dark = (30, 130, 40, 255)
    # Left leaf
    draw.polygon([(128, 80), (60, 65), (75, 95)], fill=leaf_c)
    draw.line([(128, 80), (60, 65)], fill=leaf_dark, width=2)
    # Right leaf
    draw.polygon([(128, 80), (196, 65), (181, 95)], fill=leaf_c)
    draw.line([(128, 80), (196, 65)], fill=leaf_dark, width=2)
    # Center leaf
    draw.polygon([(110, 80), (128, 45), (146, 80)], fill=leaf_c)
    # Stem
    draw.line([(128, 55), (135, 30)], fill=leaf_dark, width=6)

    img.save(os.path.join(out_dir, "Strawberry.png"))

# ==================== 2. CARROT 🥕 ====================
def draw_carrot():
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Carrot body (slanted slightly for dynamism)
    # Top at (110, 85) to (170, 75), tip at (105, 230)
    body = [
        (95, 90),
        (165, 75),
        (160, 115),
        (145, 165),
        (125, 215),
        (110, 235), # tip
        (100, 220),
        (90, 165),
        (85, 120),
    ]
    # Shadow/edge
    draw.polygon([(x + 2, y + 3) for x, y in body], fill=(195, 80, 10, 255))
    # Main orange
    draw.polygon(body, fill=(255, 125, 20, 255))
    draw.ellipse([90, 75, 170, 105], fill=(255, 125, 20, 255))

    # Horizontal ridges
    ridges = [
        ((100, 115), (150, 105)),
        ((96, 145), (142, 135)),
        ((98, 175), (132, 170)),
        ((103, 200), (124, 196)),
    ]
    for start, end in ridges:
        draw.line([start, end], fill=(210, 90, 15, 255), width=4)

    # Highlight
    draw.line([(96, 100), (105, 210)], fill=(255, 190, 120, 160), width=6)

    # Green leafy tops
    leaf_c = (55, 195, 60, 255)
    leaf_dark = (35, 145, 40, 255)
    # Left plume
    draw.line([(120, 80), (80, 35)], fill=leaf_c, width=7)
    draw.line([(80, 35), (65, 45)], fill=leaf_dark, width=4)
    # Middle plume
    draw.line([(130, 80), (130, 25)], fill=leaf_c, width=7)
    draw.line([(130, 25), (145, 35)], fill=leaf_dark, width=4)
    # Right plume
    draw.line([(140, 80), (180, 35)], fill=leaf_c, width=7)
    draw.line([(180, 35), (195, 45)], fill=leaf_dark, width=4)

    img.save(os.path.join(out_dir, "Carrot.png"))

# ==================== 3. CORN 🌽 ====================
def draw_corn():
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Corn Cob Body (yellow rounded capsule)
    cob_rect = [85, 60, 171, 210]
    draw.rounded_rectangle(cob_rect, radius=40, fill=(255, 205, 20, 255), outline=(210, 160, 10, 255), width=3)

    # Individual rounded kernels in grid pattern
    for row in range(5):
        ky = 75 + row * 24
        for col in range(3):
            kx = 96 + col * 22
            # kernel bead
            draw.ellipse([kx, ky, kx + 18, ky + 18], fill=(255, 225, 50, 255), outline=(220, 175, 15, 255), width=2)
            # kernel shine
            draw.ellipse([kx + 3, ky + 3, kx + 7, ky + 7], fill=(255, 255, 200, 220))

    # Green Leaves/Husks wrapping the sides
    husk_c = (70, 190, 60, 255)
    husk_dark = (45, 140, 40, 255)

    # Left husk
    draw.polygon([(128, 220), (65, 175), (60, 120), (85, 150), (95, 210)], fill=husk_c)
    draw.line([(128, 220), (60, 120)], fill=husk_dark, width=3)

    # Right husk
    draw.polygon([(128, 220), (191, 175), (196, 120), (171, 150), (161, 210)], fill=husk_c)
    draw.line([(128, 220), (196, 120)], fill=husk_dark, width=3)

    # Stalk bottom
    draw.line([(128, 210), (128, 235)], fill=(130, 100, 40, 255), width=10)

    img.save(os.path.join(out_dir, "Corn.png"))

# ==================== 4. TOMATO 🍅 ====================
def draw_tomato():
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Tomato body (plump rounded circle with subtle double lobe)
    draw.ellipse([50, 75, 150, 215], fill=(240, 40, 40, 255))
    draw.ellipse([106, 75, 206, 215], fill=(240, 40, 40, 255))
    draw.ellipse([65, 85, 191, 220], fill=(240, 40, 40, 255))

    # Bottom shadow
    draw.ellipse([70, 170, 186, 222], fill=(190, 25, 30, 180))

    # Huge glossy cartoon shine highlight
    draw.ellipse([75, 95, 115, 135], fill=(255, 170, 175, 180))
    draw.ellipse([82, 102, 98, 118], fill=(255, 255, 255, 240))
    draw.ellipse([105, 125, 115, 135], fill=(255, 255, 255, 200))

    # Star leaf calyx (5 points)
    stem_c = (55, 195, 55, 255)
    stem_dark = (35, 140, 35, 255)
    star_points = [
        (128, 80), (90, 60), (115, 85),
        (75, 95), (118, 95),
        (100, 115), (128, 95),
        (156, 115), (138, 95),
        (181, 95), (141, 85),
        (166, 60), (128, 80)
    ]
    draw.polygon(star_points, fill=stem_c)

    # Curly stem on top
    draw.line([(128, 80), (132, 50), (145, 42)], fill=stem_dark, width=6)

    img.save(os.path.join(out_dir, "Tomato.png"))

# ==================== 5. POTATO 🥔 ====================
def draw_potato():
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Potato body (cute organic tilted rounded oval)
    potato_pts = [
        (85, 80), (150, 70), (195, 95), (205, 140),
        (190, 185), (140, 215), (90, 210), (60, 170),
        (55, 120), (85, 80)
    ]
    # Shadow layer
    draw.polygon([(x + 2, y + 4) for x, y in potato_pts], fill=(135, 85, 45, 255))
    # Main golden-russet potato
    draw.polygon(potato_pts, fill=(195, 145, 85, 255))

    # Rounded fills for smooth soft potato shape
    draw.ellipse([60, 85, 185, 205], fill=(195, 145, 85, 255))
    draw.ellipse([90, 75, 200, 185], fill=(195, 145, 85, 255))

    # Soft top-left highlight
    draw.ellipse([75, 85, 145, 145], fill=(225, 180, 125, 140))
    draw.ellipse([85, 95, 120, 125], fill=(245, 210, 160, 160))

    # Potato eyes / dimples (little cute freckles with shadow)
    eyes = [
        (90, 115), (145, 105), (170, 135),
        (80, 160), (125, 155), (160, 180),
        (110, 190)
    ]
    for ex, ey in eyes:
        # Dark dimple
        draw.arc([ex - 6, ey - 3, ex + 6, ey + 3], start=0, end=180, fill=(125, 75, 35, 255), width=3)
        # Highlight underneath dimple
        draw.arc([ex - 5, ey, ex + 5, ey + 4], start=0, end=180, fill=(240, 205, 150, 200), width=2)

    img.save(os.path.join(out_dir, "Potato.png"))

draw_strawberry()
draw_carrot()
draw_corn()
draw_tomato()
draw_potato()

print("All 5 fruit & vegetable sprites successfully created!")
