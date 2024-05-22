<script lang="ts">
    import { fetchChats, generateInviteCode, handleJoinRoom, handleRoomSubmit } from '$lib/handlers/groupHandler';
    import {fade, fly} from 'svelte/transition';
    let activeTab = 'create';
    let modalBody: any;
    let newGroupName: string;
    let inviteCode: string;
    let isNewGroupNameValid = true;
    let isInviteCodeValid = true;
    let createGroupError: string;
    let joinGroupError: string;

    export let groupModalActive: boolean;
    export let groupChats: any =[];

    async function createNewChat(event: Event){
    event.preventDefault();
    const formData = {
        newGroupName,
        inviteCode
    }
    if(formData.newGroupName == null || formData.newGroupName == undefined || formData.newGroupName.length < 1 ){
        isNewGroupNameValid = false;
        return;
    }
    if(formData.inviteCode == null || formData.inviteCode == undefined){
        formData.inviteCode = await generateInviteCode(8);
    }
    if(formData.inviteCode.length < 8 && formData.inviteCode.length > 1){
        isInviteCodeValid = false;
        return;
    }
    isInviteCodeValid = true;
    try {
        const response = await handleRoomSubmit(formData);
        
        if (response.ok){
            groupChats = await fetchChats();
            groupModalActive = false;
            newGroupName = "";
            inviteCode = "";
            groupModalActive = false;
        }
        else{
            createGroupError = await response.text();
        }
    } catch (error) {
        console.error(error);
        return;
    }
    
}

    async function joinGroup(event: Event){
        event.preventDefault();

        if(inviteCode.length < 8 && inviteCode.length > 1){
            isInviteCodeValid = false;
            return;
        }
        isInviteCodeValid = true;
        const response = await handleJoinRoom(inviteCode);
        if (response.ok){
            groupChats = await fetchChats();
            inviteCode = "";
            groupModalActive = false;
        }
        else{
            joinGroupError = await response.text();
        }
 
    }
</script>

<div class="modal" transition:fade={{duration: 200}}>
    <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content">
            <div class="modal-header p-0">
                <div style="border-top-left-radius: 0.3rem;border-top-right-radius: 0.3rem; "class="d-flex flex-row justify-content-between w-100">
                    <h5 style="border-top-left-radius: 0.3rem !important;" class="modal-title btn w-50 h-50 pt-1 pb-1 {activeTab === 'create' ? 'active' : ''}" on:click={() => activeTab = 'create'}>Създай група</h5>
                    <h5 style="border-top-right-radius: 0.3rem !important;" class="modal-title btn w-50 h-50 pt-1 pb-1 {activeTab === 'join' ? 'active' : ''}" on:click={() => activeTab = 'join'}>Присъедини се</h5>
                </div>
            </div>
            <div class="modal-body" bind:this={modalBody}>
                {#if activeTab === 'create'}
                    <!-- Content for 'Create a chat' -->
                    <form on:submit|preventDefault={createNewChat} in:fly={{x: -200, duration: 500}} on:introstart={() => modalBody.style.overflowY = 'hidden'} on:outroend={() => modalBody.style.overflowY = 'auto'}>
                        <div class="mb-3">
                            <label for="newGroupName" class="form-label">Име на групата</label>
                            <input type="text" bind:value={newGroupName} class="form-control" id="newGroupName" class:invalid-input={!isNewGroupNameValid} required>
                            {#if !isNewGroupNameValid}
                                <div class="invalid-feedback">
                                    Името на групата трябва да е поне 1 символ.
                                </div>
                            {/if}
                        </div>
                        <div class="mb-3">
                            <label for="inviteCodeCreate" class="form-label">Код за покана</label>
                            <input type="text" maxlength="8" placeholder="(незадължително)" bind:value={inviteCode} class="form-control" id="inviteCodeCreate" class:invalid-input={!isInviteCodeValid}>
                            {#if !isInviteCodeValid}
                                <div class="invalid-feedback">
                                    Кодът за покана трябва да е 8 символа.
                                </div>
                            {/if}
                        </div>
                    </form>
                    {#if createGroupError}
                        <div class="invalid-feedback" role="alert">
                            {createGroupError}
                        </div>
                    {/if}
                {:else if activeTab === 'join'}
                    <!-- Content for 'Join a chat' -->
                    <form on:submit|preventDefault={joinGroup} in:fly={{x: 200, duration: 500}} on:introstart={() => modalBody.style.overflowY = 'hidden'} on:outroend={() => modalBody.style.overflowY = 'auto'}>
                        <div class="mb-3">
                            <label for="inviteCodeJoin" class="form-label">Invite Code</label>
                            <input type="text" maxlength="8" bind:value={inviteCode} class="form-control" id="inviteCodeJoin" class:invalid-input={!isInviteCodeValid || joinGroupError}>
                            {#if !isInviteCodeValid}
                                <div class="invalid-feedback">
                                    Кодът трябва да е 8 символа.
                                </div>
                            {:else if joinGroupError}
                                <div class="invalid-feedback" role="alert">
                                    {joinGroupError}
                                </div>
                            {/if}
                        </div>
                    </form>
                {/if}
            </div>
            <div class="modal-footer">
                {#if activeTab === 'create'}
                    <button type="submit" class="btn btn-primary" on:click|preventDefault={createNewChat}>Създай</button>
                {:else if activeTab === 'join'}
                    <button type="submit" class="btn btn-primary" on:click|preventDefault={joinGroup}>Присъедини</button>
                {/if}
                <button type="button" class="btn btn-secondary" on:click={() => groupModalActive = false}>Затвори</button>
            </div>
        </div>
    </div>
</div>

<style>
    .modal {                                              
            background-color: rgba(0, 0, 0, 0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }
            .modal-content{
                width: 20rem;
            }

            h5.modal-title{
                border-radius: 0!important;
            }
            .modal-body {
                height: 300px;
                overflow:hidden;
                overflow-x:hidden; /* Adjust this value as needed */
                overflow-y: auto; /* Enable scrolling if the content is taller than the modal body */
            }
            .invalid-input{
                border: 1px solid red;
            }
            .invalid-feedback{
                display: block;
            }
            .btn.active {
                    background-color: blue; /* Change this to your preferred color */
                    color: white; /* Change this to your preferred color */
                }
</style>