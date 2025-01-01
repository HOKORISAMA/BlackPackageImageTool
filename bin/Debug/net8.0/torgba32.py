from PIL import Image

def png_to_rgba32(input_file, output_file):
    """
    Converts a PNG image to RGBA32 (32 bits per pixel) PNG.

    Args:
        input_file (str): Input PNG file path.
        output_file (str): Output RGBA32 PNG file path.
    """
    # Open the PNG image using PIL
    with Image.open(input_file) as img:
        # Ensure the image is in RGBA mode
        img = img.convert('RGBA')

        # Save the image as RGBA32 PNG
        img.save(output_file, 'PNG', bits=(8, 8, 8, 8))

# Example usage
png_to_rgba32('A0_CG01.png', 'output_rgba32.png')