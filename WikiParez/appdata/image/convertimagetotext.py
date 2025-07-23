import pytesseract
from PIL import Image
import pyautogui
import re

region = (46,80,350,17)
screenshot = pyautogui.screenshot(region=region)

text = pytesseract.image_to_string(screenshot)
print(text)
