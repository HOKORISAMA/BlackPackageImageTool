from PIL import Image

def png_to_rgba32(input_file, output_file):
    with Image.open(input_file) as img:
        img = img.convert('RGBA')
        img.save(output_file, 'PNG', bits=(8, 8, 8, 8))

png_to_rgba32('A0_CG01.png', 'output_rgba32.png')
