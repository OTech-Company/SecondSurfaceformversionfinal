import asyncio
import bluetooth
from bleak import BleakScanner


# Scan BLE devices using Bleak
async def scanBLE():
    devices = await BleakScanner.discover()
    bleDevices = {
        d.address: d.name for d in devices
    }  # Store unique addresses in a dictionary
    return bleDevices


# Scan Classic devices using PyBluez
def scanClassic():
    devices = bluetooth.discover_devices(lookup_names=True, duration=8)
    classicDevices = {
        addr: name for addr, name in devices
    }  # Store unique addresses in a dictionary
    return classicDevices


# Run both scans
def main():
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    bleDevices = loop.run_until_complete(scanBLE())
    loop.close()
    classicDevices = scanClassic()

    # Combine both BLE and Classic devices, ensuring uniqueness by address
    allDevices = {**bleDevices, **classicDevices}

    # Combine both lists into detailed format
    detailedDevices = []
    for addr, name in allDevices.items():
        name = name if name else "Unknown"
        detailedDevices.append(
            {
                "MAC Address": addr,
                "Device Name": name,
            }
        )

    return detailedDevices
