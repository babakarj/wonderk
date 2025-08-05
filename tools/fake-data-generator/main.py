from mimesis import Person, Address
from mimesis.locales import Locale
import random
import xml.etree.ElementTree as ET
from xml.dom import minidom


def main():
    print("Hello from fake-data-generator!")

    person = Person(Locale.NL)
    address = Address(Locale.NL)

    container = ET.Element(
        "Container",
        {
            "xmlns:xsi": "http://www.w3.org/2001/XMLSchema-instance",
            "xmlns:xsd": "http://www.w3.org/2001/XMLSchema",
        },
    )

    ET.SubElement(container, "Id").text = "68465468"
    ET.SubElement(container, "ShippingDate").text = "2016-07-22T00:00:00+02:00"

    parcels = ET.SubElement(container, "parcels")

    for _ in range(4200):
        parcel = ET.SubElement(parcels, "Parcel")
        receipient = ET.SubElement(parcel, "Receipient")
        ET.SubElement(receipient, "Name").text = person.full_name()

        addr = ET.SubElement(receipient, "Address")
        ET.SubElement(addr, "Street").text = address.street_name()
        ET.SubElement(addr, "HouseNumber").text = str(random.randint(1, 999))
        ET.SubElement(addr, "PostalCode").text = address.postal_code()
        ET.SubElement(addr, "City").text = address.city()

        ET.SubElement(parcel, "Weight").text = f"{random.uniform(0.01, 5.00):.2f}"
        ET.SubElement(parcel, "Value").text = f"{random.uniform(10.0, 50000.0):.1f}"

    rough_string = ET.tostring(container, encoding="utf-8")
    reparsed = minidom.parseString(rough_string)
    pretty_xml = reparsed.toprettyxml(indent="  ")

    with open("parcels_dutch.xml", "w", encoding="utf-8") as f:
        f.write(pretty_xml)


if __name__ == "__main__":
    main()
